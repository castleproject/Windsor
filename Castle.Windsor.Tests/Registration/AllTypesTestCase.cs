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

namespace Castle.MicroKernel.Tests.Registration
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests;

	using CastleTests;

	using NUnit.Framework;

	[TestFixture]
	public class AllTypesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void RegisterAssemblyTypes_BasedOn_RegisteredInContainer()
		{
			Kernel.Register(AllTypes
			                	.FromThisAssembly()
			                	.BasedOn(typeof(ICommon))
				);

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypesFromThisAssembly_BasedOn_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromThisAssembly().BasedOn(typeof(ICommon)));

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);
		}

#if !SILVERLIGHT
		[Test]
		public void RegisterDirectoryAssemblyTypes_BasedOn_RegisteredInContainer()
		{
			var directory = AppDomain.CurrentDomain.BaseDirectory;
			Kernel.Register(AllTypes.FromAssemblyInDirectory(new AssemblyFilter(directory))
			                	.BasedOn<ICommon>());

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);
		}
#endif

		[Test]
		public void RegisterAssemblyTypes_NoService_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>());

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_FirstInterfaceService_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>()
			                	.WithService.FirstInterface()
				);

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_LookupInterfaceService_RegisteredInContainer()
		{
			Kernel.Register(Classes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>()
			                	.WithService.FromInterface()
				);

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);

			handlers = Kernel.GetHandlers(typeof(ICommonSub1));
			Assert.AreNotEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommonSub1));
			Assert.AreNotEqual(0, handlers.Length);

			handlers = Kernel.GetHandlers(typeof(ICommonSub2));
			Assert.AreNotEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommonSub2));
			Assert.AreNotEqual(0, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_DefaultService_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>()
			                	.WithService.Base()
				);

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_WithConfiguration_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>()
			                	.Configure(delegate(ComponentRegistration component)
			                	{
			                		component.LifeStyle.Transient
			                			.Named(component.Implementation.FullName + "XYZ");
			                	})
				);

			foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
			{
				Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
				Assert.AreEqual(handler.ComponentModel.Implementation.FullName + "XYZ", handler.ComponentModel.Name);
			}
		}

		[Test]
		public void RegisterAssemblyTypes_WithConfigurationBasedOnImplementation_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>()
			                	.Configure(delegate(ComponentRegistration component)
			                	{
			                		component.LifeStyle.Transient
			                			.Named(component.Implementation.FullName + "XYZ");
			                	})
			                	.ConfigureFor<CommonImpl1>(
			                		component => component.DependsOn(Property.ForKey("key1").Eq(1)))
			                	.ConfigureFor<CommonImpl2>(
			                		component => component.DependsOn(Property.ForKey("key2").Eq(2)))
				);

			foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
			{
				Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
				Assert.AreEqual(handler.ComponentModel.Implementation.FullName + "XYZ", handler.ComponentModel.Name);

				if (handler.ComponentModel.Implementation == typeof(CommonImpl1))
				{
					Assert.AreEqual(1, handler.ComponentModel.CustomDependencies.Count);
					Assert.IsTrue(handler.ComponentModel.CustomDependencies.Contains("key1"));
				}
				else if (handler.ComponentModel.Implementation == typeof(CommonImpl2))
				{
					Assert.AreEqual(1, handler.ComponentModel.CustomDependencies.Count);
					Assert.IsTrue(handler.ComponentModel.CustomDependencies.Contains("key2"));
				}
			}
		}

		[Test]
		public void RegisterGenericTypes_BasedOnGenericDefinition_RegisteredInContainer()
		{
			Kernel.Register(Classes.From(typeof(DefaultRepository<>))
			                	.Pick()
			                	.WithService.FirstInterface()
				);

			var repository = Kernel.Resolve<ClassComponents.IRepository<CustomerImpl>>();
			Assert.IsNotNull(repository);
		}

		[Test]
		public void RegisterGenericTypes_WithGenericDefinition_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn(typeof(IValidator<>))
			                	.WithService.Base()
				);

			var handlers = Kernel.GetHandlers(typeof(IValidator<ICustomer>));
			Assert.AreNotEqual(0, handlers.Length);
			var validator = Kernel.Resolve<IValidator<ICustomer>>();
			Assert.IsNotNull(validator);

			handlers = Kernel.GetHandlers(typeof(IValidator<CustomerChain1>));
			Assert.AreNotEqual(0, handlers.Length);
			var validator2 = Kernel.Resolve<IValidator<CustomerChain1>>();
			Assert.IsNotNull(validator2);
		}

		[Test]
		public void RegisterAssemblyTypes_ClosedGenericTypes_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn(typeof(IMapper<>))
			                	.WithService.FirstInterface()
				);

			var handlers = Kernel.GetHandlers(typeof(IMapper<CommonImpl1>));
			Assert.AreEqual(1, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(IMapper<CommonImpl2>));
			Assert.AreEqual(1, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_IfCondition_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromThisAssembly()
			                	.BasedOn<ICustomer>()
			                	.If(t => t.FullName.Contains("Chain"))
				);

			var handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
			Assert.AreNotEqual(0, handlers.Length);

			foreach (var handler in handlers)
			{
				Assert.IsTrue(handler.ComponentModel.Implementation.FullName.Contains("Chain"));
			}
		}

		[Test]
		public void RegisterAssemblyTypes_MultipleIfCondition_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromThisAssembly()
			                	.BasedOn<ICustomer>()
			                	.If(t => t.Name.EndsWith("2"))
			                	.If(t => t.FullName.Contains("Chain"))
				);

			var handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
			Assert.AreEqual(1, handlers.Length);
			Assert.AreEqual(typeof(CustomerChain2), handlers.Single().ComponentModel.Implementation);
		}

		[Test]
		public void RegisterAssemblyTypes_UnlessCondition_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICustomer>()
			                	.Unless(t => typeof(CustomerChain1).IsAssignableFrom(t))
				);

			foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICustomer)))
			{
				Assert.IsFalse(typeof(CustomerChain1).IsAssignableFrom(handler.ComponentModel.Implementation));
			}
		}

		[Test]
		public void RegisterAssemblyTypes_MultipleUnlessCondition_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICustomer>()
			                	.Unless(t => t.Name.EndsWith("2"))
			                	.Unless(t => t.Name.EndsWith("3"))
				);

			var handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
			Assert.IsNotEmpty(handlers);
			foreach (var handler in handlers)
			{
				var name = handler.ComponentModel.Implementation.Name;
				Assert.IsFalse(name.EndsWith("2"));
				Assert.IsFalse(name.EndsWith("3"));
			}
		}

#if(!SILVERLIGHT)
		[Test]
		public void RegisterTypes_WithLinq_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.From(from type in Assembly.GetExecutingAssembly().GetExportedTypes()
			                              where type.IsDefined(typeof(SerializableAttribute), true)
			                              select type
			                	).BasedOn<CustomerChain1>());

			var handlers = Kernel.GetAssignableHandlers(typeof(CustomerChain1));
			Assert.AreEqual(2, handlers.Length);
		}
#endif

		[Test]
		public void RegisterAssemblyTypes_WithLinqConfiguration_RegisteredInContainer()
		{
			Kernel.Register(AllTypes
			                	.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>()
			                	.Configure(component => component.LifeStyle.Transient
			                	                        	.Named(component.Implementation.FullName + "XYZ")
			                	)
				);

			foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
			{
				Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
				Assert.AreEqual(handler.ComponentModel.Implementation.FullName + "XYZ", handler.ComponentModel.Name);
			}
		}

		[Test]
		public void RegisterAssemblyTypes_WithLinqConfigurationReturningValue_RegisteredInContainer()
		{
			Kernel.Register(AllTypes
			                	.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ICommon>()
			                	.Configure(component => component.LifeStyle.Transient)
				);

			foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
			{
				Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
			}
		}

		[Test]
		public void RegisterMultipleAssemblyTypes_BasedOn_RegisteredInContainer()
		{
#pragma warning disable 0618 //call to obsolete method
			Kernel.Register(
				Classes.FromThisAssembly()
					.BasedOn<ICommon>()
					.BasedOn<ICustomer>()
					.If(t => t.FullName.Contains("Chain"))
					.BasedOn<DefaultTemplateEngine>()
					.BasedOn<DefaultMailSenderService>()
					.BasedOn<DefaultSpamServiceWithConstructor>()
				);
#pragma warning restore

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.AreNotEqual(0, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
			Assert.AreNotEqual(0, handlers.Length);

			foreach (var handler in handlers)
			{
				Assert.IsTrue(handler.ComponentModel.Implementation.FullName.Contains("Chain"));
			}

			handlers = Kernel.GetAssignableHandlers(typeof(DefaultSpamServiceWithConstructor));
			Assert.AreEqual(1, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_WhereConditionSatisifed_RegisteredInContainer()
		{
			Kernel.Register(
				AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
					.Where(t => t.Name == "CustomerImpl")
					.WithService.FirstInterface()
				);

			var handlers = Kernel.GetHandlers(typeof(ICustomer));
			Assert.AreEqual(1, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_OnlyPublicTypes_WillNotRegisterNonPublicTypes()
		{
			Kernel.Register(
				AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
					.BasedOn<NonPublicComponent>()
				);

			var handlers = Kernel.GetHandlers(typeof(NonPublicComponent));
			Assert.AreEqual(0, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_IncludeNonPublicTypes_WillNRegisterNonPublicTypes()
		{
			Kernel.Register(
				AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
					.IncludeNonPublicTypes()
					.BasedOn<NonPublicComponent>()
				);

			var handlers = Kernel.GetHandlers(typeof(NonPublicComponent));
			Assert.AreEqual(1, handlers.Length);
		}

		[Test]
		public void RegisterAssemblyTypes_WhenTypeInNamespace_RegisteredInContainer()
		{
			Kernel.Register(
				AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
					.Where(Component.IsInNamespace("Castle.MicroKernel.Tests.ClassComponents"))
					.WithService.FirstInterface()
				);

			var handlers = Kernel.GetHandlers(typeof(ICustomer));
			Assert.IsTrue(handlers.Length > 0);
		}

		[Test]
		public void RegisterAssemblyTypes_WhenTypeInMissingNamespace_NotRegisteredInContainer()
		{
			Kernel.Register(
				AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
					.Where(Component.IsInNamespace("Castle.MicroKernel.Tests.FooBar"))
					.WithService.FirstInterface()
				);

			Assert.AreEqual(0, Kernel.GetAssignableHandlers(typeof(object)).Length);
		}

		[Test]
		public void RegisterAssemblyTypes_WhenTypeInSameNamespaceAsComponent_RegisteredInContainer()
		{
			Kernel.Register(
				AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
					.Where(Component.IsInSameNamespaceAs<CustomerImpl2>())
					.WithService.FirstInterface()
				);

			var handlers = Kernel.GetHandlers(typeof(ICustomer));
			Assert.IsTrue(handlers.Length > 0);
		}

		[Test]
		public void RegisterSpecificTypes_WithGenericDefinition_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.From(typeof(CustomerRepository))
			                	.Pick()
			                	.WithService.FirstInterface()
				);

			var repository = Kernel.Resolve<ClassComponents.IRepository<ICustomer>>();
			Assert.IsNotNull(repository);
		}

		[Test]
		public void RegisterGenericTypes_BasedOnGenericDefinitionUsingSelect_RegisteredInContainer()
		{
			Kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
			                	.BasedOn<ITask>()
			                	.WithService.Select((t, b) =>
			                	                    from type in t.GetInterfaces()
			                	                    where !type.Equals(typeof(ITask))
			                	                    select type));
			Assert.IsNotNull(Kernel.Resolve<ITask<object>>());
		}
	}
}