// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Lifestyle
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class LifestyleApiTestCase : AbstractContainerTestCase
	{
		private void LifestyleSingle(Func<ComponentRegistration<A>, IRegistration> assingLifestyle, LifestyleType expectedLifestyle)
		{
			var registration = Component.For<A>();
			Kernel.Register(assingLifestyle(registration));
			var handler = Kernel.GetHandler(typeof(A));
			Assert.AreEqual(expectedLifestyle, handler.ComponentModel.LifestyleType);
		}

		private void LifestyleMany(Func<BasedOnDescriptor, IRegistration> assingLifestyle, LifestyleType expectedLifestyle)
		{
			var registration = Classes.FromAssembly(GetCurrentAssembly()).BasedOn<A>();
			Kernel.Register(assingLifestyle(registration));
			var handler = Kernel.GetHandler(typeof(A));
			Assert.AreEqual(expectedLifestyle, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Many_component_custom()
		{
			LifestyleMany(c => c.LifestyleCustom(typeof(CustomLifestyleManager)), LifestyleType.Custom);
		}

		[Test]
		public void Many_component_custom_generic()
		{
			LifestyleMany(c => c.LifestyleCustom<CustomLifestyleManager>(), LifestyleType.Custom);
		}

		[Test]
		public void Many_component_per_thread()
		{
			LifestyleMany(c => c.LifestylePerThread(), LifestyleType.Thread);
		}

#if FEATURE_SYSTEM_WEB
		[Test]
		public void Many_component_per_web_request()
		{
			LifestyleMany(c => c.LifestylePerWebRequest(), LifestyleType.PerWebRequest);
		}
#endif

		[Test]
		public void Many_component_pooled()
		{
			LifestyleMany(c => c.LifestylePooled(), LifestyleType.Pooled);
		}

		[Test]
		public void Many_component_scoped()
		{
			LifestyleMany(c => c.LifestyleScoped(), LifestyleType.Scoped);
		}

		[Test]
		public void Many_component_bound_to_object()
		{
			LifestyleMany(c => c.LifestyleBoundTo<object>(), LifestyleType.Bound);
		}

		[Test]
		public void Many_component_singleton()
		{
			LifestyleMany(c => c.LifestyleSingleton(), LifestyleType.Singleton);
		}

		[Test]
		public void Many_component_transient()
		{
			LifestyleMany(c => c.LifestyleTransient(), LifestyleType.Transient);
		}

		[Test]
		public void Single_component_custom()
		{
			LifestyleSingle(c => c.LifestyleCustom(typeof(CustomLifestyleManager)), LifestyleType.Custom);
		}

		[Test]
		public void Single_component_custom_generic()
		{
			LifestyleSingle(c => c.LifestyleCustom<CustomLifestyleManager>(), LifestyleType.Custom);
		}

		[Test]
		public void Single_component_per_thread()
		{
			LifestyleSingle(c => c.LifestylePerThread(), LifestyleType.Thread);
		}

#if FEATURE_SYSTEM_WEB
		[Test]
		public void Single_component_per_web_request()
		{
			LifestyleSingle(c => c.LifestylePerWebRequest(), LifestyleType.PerWebRequest);
		}
#endif

		[Test]
		public void Single_component_pooled()
		{
			LifestyleSingle(c => c.LifestylePooled(), LifestyleType.Pooled);
		}

		[Test]
		public void Single_component_scoped()
		{
			LifestyleSingle(c => c.LifestyleScoped(), LifestyleType.Scoped);
		}

		[Test]
		public void Single_component_bound_to_object()
		{
			LifestyleSingle(c => c.LifestyleBoundTo<object>(), LifestyleType.Bound);
		}

		[Test]
		public void Single_component_singleton()
		{
			LifestyleSingle(c => c.LifestyleSingleton(), LifestyleType.Singleton);
		}

		[Test]
		public void Single_component_transient()
		{
			LifestyleSingle(c => c.LifestyleTransient(), LifestyleType.Transient);
		}
	}
}