#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

namespace Castle.Services.Transaction
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Core;
	using log4net;

	/// <summary>
	/// Utility class for whatever is needed to make the code better.
	/// </summary>
	internal static class Fun
	{
		public static void Fire<TEventArgs>(this EventHandler<TEventArgs> handler,
		                                    object sender, TEventArgs args)
			where TEventArgs : EventArgs
		{
			if (handler == null) return;
			handler(sender, args);
		}

		public static void AtomRead(this ReaderWriterLockSlim sem, Action a)
		{
			AtomRead(sem, a, false);
		}

		public static void AtomRead(this ReaderWriterLockSlim sem, Action a, bool upgradable)
		{
			if (sem == null) throw new ArgumentNullException("sem");
			if (a == null) throw new ArgumentNullException("a");
			
			if (!upgradable) sem.EnterReadLock();
			else sem.EnterUpgradeableReadLock();

			try
			{
				a();
			}
			finally
			{
				if (!upgradable) sem.ExitReadLock();
				else sem.ExitUpgradeableReadLock();
			}
		}

		public static T AtomRead<T>(this ReaderWriterLockSlim sem, Func<T> f)
		{
			if (sem == null) throw new ArgumentNullException("sem");
			if (f == null) throw new ArgumentNullException("f");

			sem.EnterReadLock();

			try
			{
				return f();
			}
			finally
			{
				sem.ExitReadLock();
			}
		}

		public static void AtomWrite(this ReaderWriterLockSlim sem, Action a)
		{
			if (sem == null) throw new ArgumentNullException("sem");
			if (a == null) throw new ArgumentNullException("a");

			sem.EnterWriteLock();

			try
			{
				a();
			}
			finally
			{
				sem.ExitWriteLock();
			}
		}

		/// <summary>
		/// Iterates over a sequence and performs the action on each.
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (items == null) throw new ArgumentNullException("items");
			if (action == null) throw new ArgumentNullException("action");
			foreach (var item in items) action(item);
		}

		/// <summary>
		/// Given a logger and action, performs the action and catches + logs exceptions.
		/// </summary>
		/// <returns>Whether the lambda was a success or not.</returns>
		public static Error TryLogFail(this ILog logger, Action a)
		{
			try
			{
				a();
				return Error.OK;
			}
			catch (Exception e)
			{
				logger.Error(e);
				return new Error(false, e);
			}
		}

		public static Pair<T,T2> And<T, T2>(this T first, T2 second)
		{
			return new Pair<T, T2>(first, second);
		}
	}

	/// <summary>
	/// Error monad
	/// </summary>
	internal struct Error
	{
		public static Error OK = new Error(true, null);

		private readonly Exception _Ex;
		private readonly bool _Success;

		public Error(bool success, Exception ex)
		{
			_Success = success;
			_Ex = success ? null : ex;
		}

		/// <summary>
		/// Takes a lambda what to do if the result failed. Returns the result so 
		/// that it can be managed in whatevery way is needed.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public Error Exception(Action<Exception> a)
		{
			if (!_Success) a(_Ex);
			return this;
		}

		public Error Success(Action a)
		{
			if (_Success) a();
			return this;
		}
	}

	/// <summary>
	/// Error monad
	/// </summary>
	/// <typeparam name="T">Encapsulated success-action parameter type</typeparam>
	internal struct Error<T>
	{
		private readonly Exception _Ex;
		private readonly bool _Success;
		private readonly T _Param;

		public Error(bool success, Exception ex, T param)
		{
			_Success = success;
			_Param = param;
			_Ex = success ? null : ex;
		}

		/// <summary>
		/// Takes a lambda what to do if the result failed. Returns the result so 
		/// that it can be managed in whatevery way is needed.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public Error<T> Exception(Action<Exception> a)
		{
			if (!_Success) a(_Ex);
			return this;
		}

		public Error<T> Success(Action<T> a)
		{
			if (_Success) a(_Param);
			return this;
		}
	}
}