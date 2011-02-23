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

namespace Castle.MicroKernel.Tests
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.Tests.RuntimeParameters;
	using Castle.Windsor;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	public class RuntimeParametersTestCase : AbstractContainerTestCase
	{
		protected override WindsorContainer BuildContainer()
		{
			dependencies = new Dictionary<string, object> { { "cc", new CompC(12) }, { "myArgument", "ernst" } };
			var container = new WindsorContainer();
			container.Register(Component.For<CompA>().Named("compa"),
			                   Component.For<CompB>().Named("compb"));

			return container;
		}

		private Dictionary<string, object> dependencies;

		private void AssertDependencies(CompB compb)
		{
			Assert.IsNotNull(compb, "Component B should have been resolved");

			Assert.IsNotNull(compb.Compc, "CompC property should not be null");
			Assert.IsTrue(compb.MyArgument != string.Empty, "MyArgument property should not be empty");

			Assert.AreSame(dependencies["cc"], compb.Compc, "CompC property should be the same instnace as in the hashtable argument");
			Assert.IsTrue("ernst".Equals(compb.MyArgument),
			              string.Format("The MyArgument property of compb should be equal to ernst, found {0}", compb.MyArgument));
		}

		[Test]
		public void AddingDependencyToServiceWithCustomDependency()
		{
			var kernel = new DefaultKernel();
			kernel.Register(Component.For(typeof(NeedClassWithCustomerDependency)).Named("NeedClassWithCustomerDependency"));
			kernel.Register(Component.For(typeof(HasCustomDependency)).Named("HasCustomDependency"));

			Assert.AreEqual(HandlerState.WaitingDependency, kernel.GetHandler("HasCustomDependency").CurrentState);

			var hash = new Dictionary<object, object>();
			hash["name"] = new CompA();
			((IKernelInternal)kernel).RegisterCustomDependencies("HasCustomDependency", hash);
			Assert.AreEqual(HandlerState.Valid, kernel.GetHandler("HasCustomDependency").CurrentState);

			Assert.IsNotNull(kernel.Resolve(typeof(NeedClassWithCustomerDependency)));
		}

		[Test]
		public void Missing_service_is_correctly_detected()
		{
			TestDelegate act = () =>
			                   Kernel.Resolve<CompB>(new Arguments().Insert("myArgument", 123));

			var exception = Assert.Throws<DependencyResolverException>(act);
			Assert.AreEqual(
				string.Format(
					"Missing dependency.{0}Component compb has a dependency on Castle.MicroKernel.Tests.RuntimeParameters.CompC, which could not be resolved.{0}Make sure the dependency is correctly registered in the container as a service, or provided as inline argument.",
					Environment.NewLine),
				exception.Message);
		}

		[Test]
		public void ParametersPrecedence()
		{
			((IKernelInternal)Kernel).RegisterCustomDependencies("compb", dependencies);

			var instance_with_model = Kernel.Resolve<CompB>();
			Assert.AreSame(dependencies["cc"], instance_with_model.Compc, "Model dependency should override kernel dependency");

			var deps2 = new Dictionary<string, object>();
			deps2.Add("cc", new CompC(12));
			deps2.Add("myArgument", "ayende");

			var instance_with_args = (CompB)Kernel.Resolve(typeof(CompB), deps2);

			Assert.AreSame(deps2["cc"], instance_with_args.Compc, "Should get it from resolve params");
			Assert.AreEqual("ayende", instance_with_args.MyArgument);
		}

		[Test]
		public void ResolveUsingParameters()
		{
			var compb = Kernel.Resolve(typeof(CompB), dependencies) as CompB;

			AssertDependencies(compb);
		}

		[Test]
		public void ResolveUsingParametersWithinTheHandler()
		{
			((IKernelInternal)Kernel).RegisterCustomDependencies("compb", dependencies);
			var compb = Kernel.Resolve<CompB>();

			AssertDependencies(compb);
		}

		[Test]
		public void WillAlwaysResolveCustomParameterFromServiceComponent()
		{
			Kernel.Register(Component.For(typeof(CompC)).Named("compc"));
			var c_dependencies = new Dictionary<object, object>();
			c_dependencies["test"] = 15;
			((IKernelInternal)Kernel).RegisterCustomDependencies(typeof(CompC), c_dependencies);
			var b_dependencies = new Dictionary<object, object>();
			b_dependencies["myArgument"] = "foo";
			((IKernelInternal)Kernel).RegisterCustomDependencies(typeof(CompB), b_dependencies);
			var b = Kernel.Resolve<CompB>("compb");
			Assert.IsNotNull(b);
			Assert.AreEqual(15, b.Compc.test);
		}

		[Test]
		public void WithoutParameters()
		{
			var expectedMessage =
				string.Format(
					"Can't create component 'compb' as it has dependencies to be satisfied.{0}{0}'compb' is waiting for the following dependencies:{0}- Service 'Castle.MicroKernel.Tests.RuntimeParameters.CompC' which was not registered.{0}- Parameter 'myArgument' which was not provided. Did you forget to set the dependency?{0}",
					Environment.NewLine);
			var exception = Assert.Throws(typeof(HandlerException), () => Kernel.Resolve<CompB>());
			Assert.AreEqual(expectedMessage, exception.Message);
		}
	}
}