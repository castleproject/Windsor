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

namespace Castle.Windsor.Tests
{
	using System;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.ComponentsWithAttribute;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class RegistrationWithAttributeTestCase
	{
		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
		}

		private IWindsorContainer container;

		[Test]
		public void Attribute_key_can_be_overwritten()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.IsCastleComponent)
			                   	.ConfigureFor<HasKey>(k => k.Named("changedKey")));

			Assert.IsNull(container.Kernel.GetHandler("hasKey"));
			Assert.IsNotNull(container.Kernel.GetHandler("changedKey"));
		}

		[Test]
		public void Attribute_lifestyle_can_be_overwritten()
		{
			container.Register(AllTypes.FromThisAssembly()
			                   	.Where(Component.IsCastleComponent)
			                   	.Configure(c => c.LifeStyle.Pooled));

			var handler = container.Kernel.GetHandler("keyTransient");

			Assert.AreEqual(LifestyleType.Pooled, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Attribute_registers_key_properly()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.IsCastleComponent));

			var handler = container.Kernel.GetHandler("key");

			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(HasKey), handler.Services.Single());
			Assert.AreEqual(typeof(HasKey), handler.ComponentModel.Implementation);
			Assert.AreEqual(LifestyleType.Undefined, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Attribute_registers_type_and_name()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.IsCastleComponent));

			var handler = container.Kernel.GetHandler("keyAndType");

			Assert.AreEqual(typeof(ISimpleService), handler.Services.Single());
			Assert.AreEqual(typeof(HasKeyAndType), handler.ComponentModel.Implementation);
			Assert.AreEqual(LifestyleType.Undefined, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Attribute_registers_type_properly()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.IsCastleComponent));

			var handlers = container.Kernel.GetHandlers(typeof(ISimpleService));
			Assert.IsNotEmpty(handlers);
		}

		[Test]
		public void Attribute_sets_lifestyle()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.IsCastleComponent));

			var one = container.Resolve<HasKeyTransient>("keyTransient");
			var two = container.Resolve<HasKeyTransient>("keyTransient");

			Assert.AreNotSame(one, two);
		}

		[Test]
		public void Attribute_type_can_be_overwritten()
		{
			container.Register(AllTypes.FromThisAssembly()
			                   	.Where(Component.IsCastleComponent)
			                   	.WithService.Self());

			var handler = container.Kernel.GetAssignableHandlers(typeof(HasType)).Single();

			Assert.AreEqual(typeof(HasType), handler.Services.Single());
		}

		[Test]
		public void Can_filter_types_based_on_attribute()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.IsCastleComponent));

			var handlers = container.Kernel.GetAssignableHandlers(typeof(object));

			Assert.Greater(handlers.Length, 0);
			foreach (var handler in handlers)
			{
				Assert.That(Attribute.IsDefined(handler.ComponentModel.Implementation, typeof(CastleComponentAttribute)));
			}
		}

		[Test]
		public void Can_filter_types_based_on_custom_attribute()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.HasAttribute<UserAttribute>));

			container.Resolve<HasUserAttributeRegister>();
			container.Resolve<HasUserAttributeNonRegister>();
		}

		[Test]
		public void Can_filter_types_based_on_custom_attribute_properties()
		{
			container.Register(AllTypes.FromThisAssembly().Where(Component.HasAttribute<UserAttribute>(u => u.Register)));
			container.Resolve<HasUserAttributeRegister>();
			Assert.Throws<ComponentNotFoundException>(() => container.Resolve<HasUserAttributeNonRegister>());
		}
	}
}