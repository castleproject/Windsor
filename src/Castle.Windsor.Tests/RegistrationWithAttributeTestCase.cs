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

namespace CastleTests
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
	public class RegistrationWithAttributeTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Attribute_key_can_be_overwritten()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.IsCastleComponent)
			                   	.ConfigureFor<HasKey>(k => k.Named("changedKey")));

			Assert.IsNull(Container.Kernel.GetHandler("hasKey"));
			Assert.IsNotNull(Container.Kernel.GetHandler("changedKey"));
		}

		[Test]
		public void Attribute_lifestyle_can_be_overwritten()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly())
			                   	.Where(Component.IsCastleComponent)
			                   	.LifestylePooled());

			var handler = Container.Kernel.GetHandler("keyTransient");

			Assert.AreEqual(LifestyleType.Pooled, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Attribute_registers_key_properly()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.IsCastleComponent));

			var handler = Container.Kernel.GetHandler("key");

			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(HasKey), handler.ComponentModel.Services.Single());
			Assert.AreEqual(typeof(HasKey), handler.ComponentModel.Implementation);
			Assert.AreEqual(LifestyleType.Undefined, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Attribute_registers_type_and_name()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.IsCastleComponent));

			var handler = Container.Kernel.GetHandler("keyAndType");

			Assert.AreEqual(typeof(ISimpleService), handler.ComponentModel.Services.Single());
			Assert.AreEqual(typeof(HasKeyAndType), handler.ComponentModel.Implementation);
			Assert.AreEqual(LifestyleType.Undefined, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Attribute_registers_type_properly()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.IsCastleComponent));

			var handlers = Container.Kernel.GetHandlers(typeof(ISimpleService));
			Assert.IsNotEmpty(handlers);
		}

		[Test]
		public void Attribute_sets_lifestyle()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.IsCastleComponent));

			var one = Container.Resolve<HasKeyTransient>("keyTransient");
			var two = Container.Resolve<HasKeyTransient>("keyTransient");

			Assert.AreNotSame(one, two);
		}

		[Test]
		public void Attribute_type_can_be_overwritten()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly())
			                   	.Where(Component.IsCastleComponent)
			                   	.WithService.Self());

			var handler = Container.Kernel.GetAssignableHandlers(typeof(HasType)).Single();

			Assert.AreEqual(typeof(HasType), handler.ComponentModel.Services.Single());
		}

		[Test]
		public void Can_filter_types_based_on_attribute()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.IsCastleComponent));

			var handlers = Container.Kernel.GetAssignableHandlers(typeof(object));

			Assert.Greater(handlers.Length, 0);
			foreach (var handler in handlers)
			{
				Assert.That(Attribute.IsDefined(handler.ComponentModel.Implementation, typeof(CastleComponentAttribute)));
			}
		}

		[Test]
		public void Can_filter_types_based_on_custom_attribute()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.HasAttribute<UserAttribute>));

			Container.Resolve<HasUserAttributeRegister>();
			Container.Resolve<HasUserAttributeNonRegister>();
		}

		[Test]
		public void Can_filter_types_based_on_custom_attribute_properties()
		{
			Container.Register(Classes.FromAssembly(GetCurrentAssembly()).Where(Component.HasAttribute<UserAttribute>(u => u.Register)));
			Container.Resolve<HasUserAttributeRegister>();
			Assert.Throws<ComponentNotFoundException>(() => Container.Resolve<HasUserAttributeNonRegister>());
		}
	}
}