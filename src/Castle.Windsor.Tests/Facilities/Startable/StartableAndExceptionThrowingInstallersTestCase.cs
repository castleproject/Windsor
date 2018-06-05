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

namespace CastleTests.Facilities.Startable
{
	using System;

	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Tests;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class StartableAndExceptionThrowingInstallersTestCase
	{
		[Test]
		[Bug("IOC-311")]
		public void InstallShouldThrowExceptionFromFailedInstaller()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());

				// I would expect NotImplementedException to be thrown here
				// because it is thrown in the install method of the ExceptionThrowingInstaller
				// however, what appears to be happening is that after the NotImplementedException
				// is thrown, the DependencyInstaller never runs, but the "deferred start" code
				// in OptimizeDependencyResolutionDisposable.Dispose() kicks in anyway
				// and tries to create the StartableComponent, which it fails to do
				// because IDependencyOfStartableComponent is not registered
				// The net effect is that the NotImplementedException thrown by ExceptionThrowingInstaller
				// is "swallowed" and instead I see a Kernel HandlerException telling me that
				// IDependencyOfStartableComponent is not registered

				// expected :
				Assert.Throws<NotImplementedException>(
					() =>
					container.Install(new ActionBasedInstaller(c => c.Register(Component.For<UsesIEmptyService>().Start())),
					                  new ActionBasedInstaller(c => { throw new NotImplementedException(); }),
					                  new ActionBasedInstaller(c => c.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>()))));
			}
		}

		[Test]
		[Bug("IOC-311")]
		public void StartableComponentShouldNotStartIfExceptionThrownByInstaller()
		{
			UsesIEmptyService.instancesCreated = 0;
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				Assert.Throws<NotImplementedException>(
					() =>
					container.Install(new ActionBasedInstaller(c => c.Register(Component.For<UsesIEmptyService>().Start())),
					                  new ActionBasedInstaller(c => c.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>())),
					                  new ActionBasedInstaller(c => { throw new NotImplementedException(); })));

				// In this scenario, I've registered IDependencyOfStartableComponent
				// before the ExceptionThrowingInstaller gets a chance to gum up the works
				// I would expect that the "deferred start" code NOT run here, 
				// and the StartableComponent remain un-instantiated.
				// However, Castle is creating the StartableComponent anyway
				// and then allows the NotImplementedException to bubble out.
				// Presumably, this is due to the "deferred start" mechanism
				// being implemented by a using() block or something similar
				// via OptimizeDependencyResolutionDisposable.Dispose()

				Assert.AreEqual(0, UsesIEmptyService.instancesCreated);
			}
		}
	}
}