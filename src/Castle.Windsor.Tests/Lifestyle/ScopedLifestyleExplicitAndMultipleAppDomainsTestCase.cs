// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
#if !SILVERLIGHT
	using System;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Registration;
	using CastleTests.Components;
	using NUnit.Framework;

	public class ScopedLifestyleExplicitAndMultipleAppDomainsTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.Register(Component.For<A>().LifestyleScoped());
		}

		private static AppDomain CreateAnotherAppDomain()
		{
			return AppDomain.CreateDomain("Another", null, new AppDomainSetup {ApplicationBase = AppDomain.CurrentDomain.BaseDirectory});
		}

		[Test]
		public void Context_is_preserved_if_crossed_to_other_AppDomain_in_the_meantime()
		{
			using (Container.BeginScope())
			{
				var one = Container.Resolve<A>();

				var anotherDomain = CreateAnotherAppDomain();
				Activator.CreateInstance(anotherDomain, typeof (Object).Assembly.FullName, typeof (Object).FullName).Unwrap();

				var two = Container.Resolve<A>();
				Assert.AreSame(one, two);
			}
		}
	}
#endif
}