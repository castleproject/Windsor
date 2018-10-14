// Copyright 2018 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace CastleTests
{
	using System;

	using NUnit.Framework;

	public static class ReferenceTracker
	{
		/// <summary>
		/// <para>
		/// Use a lambda method to evaluate an expression returning an object instance to track.
		/// </para>
		/// <para>
		/// (If the expression is evaluated directly inside the test method, the runtime will keep it alive
		/// until the test method returns if the runtime is in debug mode. ReSharper’s test runner runs in
		/// debug mode when the active solution configuration is Debug.)
		/// </para>
		/// </summary>
		public static ReferenceTracker<T> Track<T>(Func<T> getInstanceToTrack) where T : class
		{
			return ReferenceTracker<T>.Track(getInstanceToTrack);
		}
	}

	/// <summary>
	/// A test helper encapsulating correct reference tracking.
	/// </summary>
	public struct ReferenceTracker<T> where T : class
	{
		private readonly WeakReference weakReference;

		private ReferenceTracker(WeakReference weakReference)
		{
			this.weakReference = weakReference;
		}

		/// <summary>
		/// <para>
		/// Use a lambda method to evaluate an expression returning an object instance to track.
		/// </para>
		/// <para>
		/// (If the expression is evaluated directly inside the test method, the runtime will keep it alive
		/// until the test method returns if the runtime is in debug mode. ReSharper’s test runner runs in
		/// debug mode when the active solution configuration is Debug.)
		/// </para>
		/// </summary>
		public static ReferenceTracker<T> Track(Func<T> getInstanceToTrack)
		{
			if (getInstanceToTrack == null)
				throw new ArgumentNullException(nameof(getInstanceToTrack));

			return new ReferenceTracker<T>(new WeakReference(getInstanceToTrack.Invoke()));
		}

		/// <summary>
		/// Calls <see cref="GC.Collect()"/> and asserts that the tracked instance is still alive.
		/// </summary>
		public void AssertStillReferenced()
		{
			GC.Collect();
			if (!weakReference.IsAlive)
				Assert.Fail("The tracked instance is not longer referenced.");
		}

		/// <summary>
		/// Calls <see cref="GC.Collect()"/> and asserts that the tracked instance is no longer alive.
		/// </summary>
		public void AssertNoLongerReferenced()
		{
			GC.Collect();
			if (weakReference.IsAlive)
				Assert.Fail("The tracked instance is still referenced.");
		}

		/// <summary>
		/// <para>
		/// Calls <see cref="GC.Collect()"/> and asserts that the tracked instance is still alive and
		/// passes it to the specified action.
		/// </para>
		/// <para>
		/// Be careful not to let the tracked instance become reachable via a local variable in the test method
		/// or it will be kept alive until the end of the test method if the runtime is in debug mode.
		/// (See <see cref="Track"/>.)
		/// </para>
		/// </summary>
		public void AssertStillReferencedAndDo(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			GC.Collect();
			var target = weakReference.Target;
			if (target is null)
				Assert.Fail("The tracked instance is not longer referenced.");

			action.Invoke((T)target);
		}

		/// <summary>
		/// <para>
		/// Calls <see cref="GC.Collect()"/> and asserts that the tracked instance is still alive,
		/// passes it to the specified function, and returns the value returned by the specified function.
		/// </para>
		/// <para>
		/// Be careful not to let the tracked instance become reachable via a local variable in the test method
		/// or it will be kept alive until the end of the test method if the runtime is in debug mode.
		/// (See <see cref="Track"/>.)
		/// </para>
		/// </summary>
		public TReturn AssertStillReferencedAndDo<TReturn>(Func<T, TReturn> func)
		{
			if (func == null)
				throw new ArgumentNullException(nameof(func));

			GC.Collect();
			var target = weakReference.Target;
			if (target is null)
				Assert.Fail("The tracked instance is not longer referenced.");

			return func.Invoke((T)target);
		}
	}
}
