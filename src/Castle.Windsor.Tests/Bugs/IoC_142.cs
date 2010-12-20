// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
namespace Castle.Windsor.Tests.Bugs
{
	using Castle.Core.Configuration;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class IoC_142
	{
		public class ClassTakingNullable
		{
			public int? SomeVal { get; set; }
		}

		public class ClassTakingNullableViaCtor
		{
			public ClassTakingNullableViaCtor(double? foo)
			{
			}
		}

		[Test]
		public void ShouldBeAbleToSupplyValueForNullableParam()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<ClassTakingNullable>());

			var arguments = new Arguments().Insert("SomeVal", 5);
			var s = container.Resolve<ClassTakingNullable>(arguments);

			Assert.IsNotNull(s.SomeVal);
		}

		[Test]
		public void ShouldBeAbleToSupplyValueForNullableParamViaCtor()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<ClassTakingNullableViaCtor>());

			var s = container.Resolve<ClassTakingNullableViaCtor>(new Arguments().Insert("foo", 5d));
			Assert.IsNotNull(s);
		}

		[Test]
		public void ShouldBeAbleToSupplyValueForNullableParamViaCtor_FromConfig()
		{
			var container = new WindsorContainer();
			var configuration = new MutableConfiguration("parameters");
			configuration.CreateChild("foo", "5");
			container.Register(Component.For<ClassTakingNullableViaCtor>().Configuration(configuration));

			var s = container.Resolve<ClassTakingNullableViaCtor>();
			Assert.IsNotNull(s);
		}

		[Test]
		public void ShouldBeAbleToSupplyValueForNullableParam_FromConfig()
		{
			var container = new WindsorContainer();
			var configuration = new MutableConfiguration("parameters");
			configuration.CreateChild("SomeVal", "5");
			container.Register(Component.For<ClassTakingNullable>().Configuration(configuration));

			var s = container.Resolve<ClassTakingNullable>();
			Assert.IsNotNull(s.SomeVal);
		}
	}
}