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

namespace Castle.Facilities.WcfIntegration.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Resource;
	using Castle.DynamicProxy;
	using Castle.Facilities.Logging;
	using Castle.Facilities.TypedFactory;
	using Castle.Facilities.WcfIntegration.Async;
	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Demo;
	using Castle.Facilities.WcfIntegration.Tests.Behaviors;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Installer;
	using log4net.Appender;
	using log4net.Config;
	using log4net.Core;
	using NUnit.Framework;

    [TestFixture]
	public class WcfClientFixture
	{
		private MemoryAppender memoryAppender;

		#region Setup/Teardown

		[SetUp]
		public void TestInitialize()
		{
			windsorContainer = new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(WcfClient.ForChannels(
					new DefaultClientModel()
					{
						Endpoint = WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					}),
					Component.For<IServiceBehavior>()
						.Instance(new ServiceDebugBehavior()
						{
							IncludeExceptionDetailInFaults = true
						}),
					Component.For<NetDataContractFormatBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Explicit),
					Component.For<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"),
							WcfEndpoint.ForContract<IOperationsEx>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations/Ex")
								)
						),
					Component.For<IAmUsingWindsor>().ImplementedBy<UsingWindsor>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel()
						)
				);

			RegisterLoggingFacility(windsorContainer);
		}

		[TearDown]
		public void TestCleanup()
		{
			windsorContainer.Dispose();
			ChannelFactoryListener.Reset();
		}

		#endregion

		private IWindsorContainer windsorContainer;

		[Test]
		public void CanResolveClientInterfaceAssociatedWithChannel()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(42, client.GetValueFromConstructor());
		}

		[Test]
		public void CanResolveClientInterfaceWithOutAndRefArguments()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			int refValue = 0, outValue;
			Assert.AreEqual(42, client.GetValueFromConstructorAsRefAndOut(ref refValue, out outValue));
			Assert.AreEqual(42, refValue);
			Assert.AreEqual(42, outValue);
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingDefaultBinding()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.Named("operations")
						.AsWcfClient(new DefaultClientModel()
						{
							Endpoint = WcfEndpoint.At("net.tcp://localhost/Operations2")
						})
					))
				{
					var client = clientContainer.Resolve<IOperations>("operations");
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingSuppliedModel()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
					.Register(Component.For<IOperations>()
						.Named("operations")
						.LifeStyle.Transient
						.AsWcfClient()
					))
				{
					var client1 = clientContainer.Resolve<IOperations>("operations",
						new
						{
							Model = new DefaultClientModel
								{
									Endpoint = WcfEndpoint.BoundTo(new NetTcpBinding())
										.At("net.tcp://localhost/Operations2")
								}
						});
					var client2 = clientContainer.Resolve<IOperations>("operations",
						new
						{
							Model = new DefaultClientModel()
								{
									Endpoint = WcfEndpoint.BoundTo(new NetTcpBinding())
										.At("net.tcp://localhost/Operations2")
								}
						});
					Assert.AreEqual(28, client1.GetValueFromConstructor());
					Assert.AreEqual(28, client2.GetValueFromConstructor());
					clientContainer.Release(client1);
					clientContainer.Release(client2);
				}
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingSuppliedEndpoint()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.Named("operations")
						.LifeStyle.Transient
						.AsWcfClient()
					))
				{
					var client1 = new WeakReference(clientContainer.Resolve<IOperations>("operations",
						new { Endpoint = WcfEndpoint.At("net.tcp://localhost/Operations2") }));
					var client2 = clientContainer.Resolve<IOperations>("operations",
						new { Endpoint = WcfEndpoint.At("net.tcp://localhost/Operations2") });

					Assert.AreEqual(28, ((IOperations)client1.Target).GetValueFromConstructor());
					Assert.AreEqual(28, client2.GetValueFromConstructor());
					clientContainer.Release(client1.Target);
					clientContainer.Release(client2);
					System.GC.Collect();
					Assert.IsFalse(client1.IsAlive);
				}
			}
		}

		[Test]
		public void CanLazilyResolveClientAssociatedWithChannelUsingSuppliedModel()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero))
				{
					var client1 = clientContainer.Resolve<IOperations>(
						new
						{
							Model = new DefaultClientModel
							{
								Endpoint = WcfEndpoint.BoundTo(new NetTcpBinding())
									.At("net.tcp://localhost/Operations2")
							}
						});
					var client2 = clientContainer.Resolve<IOperations>(
						new
						{
							Model = new DefaultClientModel()
							{
								Endpoint = WcfEndpoint.BoundTo(new NetTcpBinding())
									.At("net.tcp://localhost/Operations2")
							}
						});
					Assert.AreEqual(28, client1.GetValueFromConstructor());
					Assert.AreEqual(28, client2.GetValueFromConstructor());
					clientContainer.Release(client1);
					clientContainer.Release(client2);
				}
			}
		}

		[Test]
		public void CanLazilyResolveClientAssociatedWithChannelUsingSuppliedEndpoint()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero))
				{
					var client1 = new WeakReference(clientContainer.Resolve<IOperations>("operations",
						new { Endpoint = WcfEndpoint.At("net.tcp://localhost/Operations2") }));
					var client2 = clientContainer.Resolve<IOperations>("operations",
						new { Endpoint = WcfEndpoint.At("net.tcp://localhost/Operations2") });

					Assert.AreEqual(28, ((IOperations)client1.Target).GetValueFromConstructor());
					Assert.AreEqual(28, client2.GetValueFromConstructor());
					clientContainer.Release(client1.Target);
					clientContainer.Release(client2);
					System.GC.Collect();
					Assert.IsFalse(client1.IsAlive);
				}
			}
		}

		[Test, ExpectedException(typeof(FacilityException),
			ExpectedMessage = "The IWcfClientFactory is only available with the TypedFactoryFacility.  Did you forget to register that facility? Also make sure that TypedFactoryFacility was registred before WcfFacility.")]
		public void WillGetFriendlyErrorWhenFactoryIsNotAvailable()
		{
			using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero))
			{
				clientContainer.Resolve<IWcfClientFactory>();
			}
		}


		[Test, ExpectedException(typeof(FacilityException),
			ExpectedMessage = "The IWcfClientFactory is only available with the TypedFactoryFacility.  Did you forget to register that facility? Also make sure that TypedFactoryFacility was registred before WcfFacility.")]
		public void WillGetFriendlyErrorWhenFactoryIsNotAvailable_because_TypedFactoryFacility_was_registered_after_WCFFacility()
		{
			using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
					.AddFacility<TypedFactoryFacility>())
			{
				clientContainer.Resolve<IWcfClientFactory>();
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingFactoryWithConfiguration()
		{
			using (var clientContainer = new WindsorContainer()
					.AddFacility<TypedFactoryFacility>()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero))
			{
				var factory = clientContainer.Resolve<IWcfClientFactory>();

				var client = factory.GetClient<IAmUsingWindsor>("WSHttpBinding_IAmUsingWindsor");

				Assert.AreEqual(42, client.GetValueFromWindsorConfig());
				factory.Release(client);
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingFactoryWithModel()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<TypedFactoryFacility>()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero))
				{
					var factory = clientContainer.Resolve<IWcfClientFactory>();

					var client = factory.GetClient<IOperations>(new DefaultClientModel
					{
						Endpoint = WcfEndpoint.BoundTo(new NetTcpBinding())
							.At("net.tcp://localhost/Operations2")
					});

					Assert.AreEqual(28, client.GetValueFromConstructor());
					factory.Release(client);
				}
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingFactoryWithEndpoint()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<TypedFactoryFacility>()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero))
				{
					var factory = clientContainer.Resolve<IWcfClientFactory>();

					var client = factory.GetClient<IOperations>(WcfEndpoint.At("net.tcp://localhost/Operations2"));

					Assert.AreEqual(28, client.GetValueFromConstructor());
					factory.Release(client);
				}
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingFactoryWithUri()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<TypedFactoryFacility>()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero))
				{
					var factory = clientContainer.Resolve<IWcfClientFactory>();

					var client = factory.GetClient<IOperations>(new Uri("net.tcp://localhost/Operations2"));

					Assert.AreEqual(28, client.GetValueFromConstructor());
					factory.Release(client);
				}
			}
		}

		[Test]
		public void WillRecoverFromAnUnhandledExceptionWithChannelUsingSuppliedModel()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperationsEx>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
					.Register(Component.For<IOperationsEx>()
						.Named("operations")
						.LifeStyle.Transient
						.AsWcfClient()
					))
				{
					var client = clientContainer.Resolve<IOperationsEx>("operations",
						new
						{
							Model = new DefaultClientModel
							{
								Endpoint = WcfEndpoint.BoundTo(new NetTcpBinding())
									.At("net.tcp://localhost/Operations2")
							}
						});
					try
					{
						client.ThrowException();
					}
					catch (Exception)
					{
						client.Backup(new Dictionary<string, object>());
						client.Backup(new Dictionary<string, object>());
					}
					clientContainer.Release(client);
				}
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelFromConfiguration()
		{
			windsorContainer.Register(Component.For<IAmUsingWindsor>()
				.Named("usingWindsor")
				.AsWcfClient(new DefaultClientModel()
				{
					Endpoint = WcfEndpoint
						.FromConfiguration("WSHttpBinding_IAmUsingWindsor")
				}));

			var client = windsorContainer.Resolve<IAmUsingWindsor>("usingWindsor");
			Assert.AreEqual(42, client.GetValueFromWindsorConfig());
		}

		[Test]
		public void CanResolveClientWhenListedInTheFacility()
		{
			windsorContainer.Register(Component.For<ClassNeedingService>());
			ClassNeedingService component = windsorContainer.Resolve<ClassNeedingService>();
			Assert.IsNotNull(component.Operations);
			Assert.AreEqual(42, component.Operations.GetValueFromConstructor());
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingRelativeAddress()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddBaseAddresses("net.tcp://localhost/Operations")
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("Extended")
							)
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
					.Register(Component.For<IOperations>()
						.Named("operations")
						.AsWcfClient(new DefaultClientModel()
						{
							Endpoint = WcfEndpoint
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations/Extended")

						})
					))
				{
					var client = clientContainer.Resolve<IOperations>("operations");
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelUsingViaAddress()
		{
			using (var localContainer = new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<IOperations>()
						.ImplementedBy<Operations>()
						.DependsOn(new { number = 22 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("urn:castle:operations")
								.Via("net.tcp://localhost/OperationsVia")
								)
						),
					Component.For<IOperations>()
						.Named("operations")
						.AsWcfClient(new DefaultClientModel()
						{
							Endpoint = WcfEndpoint
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("urn:castle:operations")
								.Via("net.tcp://localhost/OperationsVia")
						})
					))
			{
				var client = localContainer.Resolve<IOperations>("operations");
				Assert.AreEqual(22, client.GetValueFromConstructor());
			}
		}

		[Test]
		public void WillApplyOperationBehaviors()
		{
			windsorContainer.Register(
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations/Ex")
							.AddExtensions(typeof(NetDataContractFormatBehavior))
					})
				);

			var client = windsorContainer.Resolve<IOperationsEx>("operations");
			client.Backup(new Dictionary<string, object>());
		}

		[Test]
		public void WillApplyExlpicitScopedKeyEndpointBehaviors()
		{
			CallCountEndpointBehavior.CallCount = 0;
			windsorContainer.Register(
				Component.For<CallCountEndpointBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit)
					.Named("specialBehavior")
					.LifeStyle.Transient,
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
						   {
							   Endpoint = WcfEndpoint
								   .BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								   .At("net.tcp://localhost/Operations/Ex")
								   .AddExtensions("specialBehavior")
						   })
				);
			var client = windsorContainer.Resolve<IOperationsEx>("operations");
			client.Backup(new Dictionary<string, object>());
			Assert.AreEqual(1, CallCountEndpointBehavior.CallCount);
		}

		[Test]
		public void WillApplyExlpicitScopedServiceEndpointExtensions()
		{
			CallCountEndpointBehavior.CallCount = 0;
			windsorContainer.Register(
				Component.For<CallCountEndpointBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit)
					.Named("specialBehavior")
					.LifeStyle.Transient,
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations/Ex")
							.AddExtensions(typeof(CallCountEndpointBehavior))
					})
				);
			var client = windsorContainer.Resolve<IOperationsEx>("operations");
			client.Backup(new Dictionary<string, object>());
			Assert.AreEqual(1, CallCountEndpointBehavior.CallCount);
		}

		[Test]
		public void WillApplyChannelFactoryAwareExtensions()
		{
			windsorContainer.Register(
				Component.For<ChannelFactoryListener>(),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");

			Assert.IsTrue(ChannelFactoryListener.CreatedCalled);
			Assert.IsTrue(ChannelFactoryListener.OpeningCalled);
			Assert.IsTrue(ChannelFactoryListener.OpenedCalled);
			client.GetValueFromConstructor();
			Assert.IsFalse(ChannelFactoryListener.ClosingCalled);
			Assert.IsFalse(ChannelFactoryListener.ClosedCalled);

			windsorContainer.Kernel.RemoveComponent("operations");
			Assert.IsTrue(ChannelFactoryListener.ClosingCalled);
			Assert.IsTrue(ChannelFactoryListener.ClosedCalled);
		}

		[Test]
		public void WillApplyChannelFactoryAwareExtensionsOnModel()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					}.AddExtensions(new ChannelFactoryListener()))
				);

			var client = windsorContainer.Resolve<IOperations>("operations");

			Assert.IsTrue(ChannelFactoryListener.CreatedCalled);
			Assert.IsTrue(ChannelFactoryListener.OpeningCalled);
			Assert.IsTrue(ChannelFactoryListener.OpenedCalled);
			client.GetValueFromConstructor();
			Assert.IsFalse(ChannelFactoryListener.ClosingCalled);
			Assert.IsFalse(ChannelFactoryListener.ClosedCalled);

			windsorContainer.Kernel.RemoveComponent("operations");
			Assert.IsTrue(ChannelFactoryListener.ClosingCalled);
			Assert.IsTrue(ChannelFactoryListener.ClosedCalled);
		}


		[Test]
		public void WillApplyChannelFactoryAwareExtensionsWhenChannelCreated()
		{
			windsorContainer.Register(
				Component.For<ChannelFactoryListener>(),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			CollectionAssert.IsEmpty(ChannelFactoryListener.ChannelsCreated);

			var client = windsorContainer.Resolve<IOperations>("operations");

			Assert.AreEqual(1, ChannelFactoryListener.ChannelsCreated.Count);
			Assert.AreEqual(1, ChannelFactoryListener.ChannelsAvailable.Count);
			CollectionAssert.AreEqual(new[] { client }, ChannelFactoryListener.ChannelsAvailable);
		}

		[Test]
		public void WillRecoverFromAnUnhandledException()
		{
			windsorContainer.Register(
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations/Ex")

					})
				);

			var client = windsorContainer.Resolve<IOperationsEx>("operations");
			try
			{
				client.ThrowException();
				Assert.Fail("Should have raised an exception");
			}
			catch (Exception)
			{
				client.Backup(new Dictionary<string, object>());
			}
		}

		[Test]
		public void WillRecoverFromAnUnhandledExceptionAsynchronously()
		{
			windsorContainer.Register(
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations/Ex")

					})
				);

			var client = windsorContainer.Resolve<IOperationsEx>("operations");
			try
			{
				client.BeginWcfCall(p => p.ThrowException()).End();
			}
			catch (Exception)
			{
				client.BeginWcfCall(p => p.Backup(new Dictionary<string, object>())).End();
			}
		}

		[Test, ExpectedException(typeof(CommunicationObjectFaultedException))]
		public void CanInhibitRecoveryFromAnUnhandledException()
		{
			using (var localContainer = new WindsorContainer()
				.AddFacility<WcfFacility>(f =>
				{
					f.CloseTimeout = TimeSpan.Zero;
					f.Clients.DefaultChannelPolicy = null;
				})
				.Register(
					Component.For<RefreshChannelPolicy>()
						.DependsOn(new { Reconnect = false }),
					Component.For<IOperationsEx>()
						.Named("operations")
						.AsWcfClient(new DefaultClientModel()
						{
							Endpoint = WcfEndpoint
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations/Ex")
						})
					))
			{
				var client = localContainer.Resolve<IOperationsEx>("operations");
				try
				{
					client.ThrowException();
				}
				catch (Exception)
				{
					client.Backup(new Dictionary<string, object>());
				}
			}
		}

		[Test]
		public void CanRecoverFromCommunicationException()
		{
			Func<IWindsorContainer> createLocalContainer = () =>
				new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations1"),
							WcfEndpoint.ForContract<IOperationsEx>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations1/Ex")
								)
						)
					);

			windsorContainer.Register(
				Component.For<ReconnectChannelPolicy>(),
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations1/Ex")
					})
				);

			IOperationsEx client = null;

			using (createLocalContainer())
			{
				client = windsorContainer.Resolve<IOperationsEx>("operations");
				client.Backup(new Dictionary<string, object>());
			}

			using (createLocalContainer())
			{
				client.Backup(new Dictionary<string, object>());
			}
		}

		[Test]
		public void CanRecoverFromCommunicationExceptionOnEndpoint()
		{
			Func<IWindsorContainer> createLocalContainer = () =>
				new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations1"),
							WcfEndpoint.ForContract<IOperationsEx>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations1/Ex")
								)
						)
					);

			windsorContainer.Register(
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(WcfEndpoint
					             	.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
					             	.At("net.tcp://localhost/Operations1/Ex")
					             	.AddExtensions(new ReconnectChannelPolicy())));

			IOperationsEx client;

			using (createLocalContainer())
			{
				client = windsorContainer.Resolve<IOperationsEx>("operations");
				client.Backup(new Dictionary<string, object>());
			}

			using (createLocalContainer())
			{
				client.Backup(new Dictionary<string, object>());
			}
		}

		[Test]
		public void CanRecoverFromCommunicationExceptionAsynchronously()
		{
			Func<IWindsorContainer> createLocalContainer = () =>
				new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations1"),
							WcfEndpoint.ForContract<IOperationsEx>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations1/Ex")
								)
						)
					);

			windsorContainer.Register(
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations1/Ex")
					})
				);
			IOperationsEx client = null;

			using (createLocalContainer())
			{
				client = windsorContainer.Resolve<IOperationsEx>("operations");
				client.BeginWcfCall(p => p.Backup(new Dictionary<string, object>())).End();
			}

			using (createLocalContainer())
			{
				client.BeginWcfCall(p => p.Backup(new Dictionary<string, object>())).End();
			}
		}

		[Test]
		public void CanResolveClientAssociatedWithChannelFromXmlConfiguration()
		{
			using (var container = new WindsorContainer()
					.Install(Configuration.FromXml(new StaticContentResource(xmlConfiguration))))
			{
				var client = container.Resolve<IAmUsingWindsor>("usingWindsor");
				Assert.AreEqual(42, client.GetValueFromWindsorConfig());
			}
		}

		[Test]
		public void CanAccessIClientChannelChannelInterface()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			var channel = client as IClientChannel;
			Assert.IsNotNull(channel);
			Assert.AreEqual(CommunicationState.Opened, channel.State);
		}

		[Test]
		public void CanUseOperationContextWithClient()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			using (new OperationContextScope(WcfContextChannel.For(client)))
			{
				MessageHeader header = MessageHeader.CreateHeader("MyHeader", "", "MyValue", false);
				OperationContext.Current.OutgoingMessageHeaders.Add(header);
				Assert.AreEqual(42, client.GetValueFromConstructor());
			}
		}

		[Test]
		public void CanCaptureRequestsAndResponses()
		{
			windsorContainer.Register(
				Component.For<LogMessageEndpointBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit)
					.Named("logMessageBehavior"),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.LogMessages()
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(42, client.GetValueFromConstructor());
			Assert.AreEqual(4, memoryAppender.GetEvents().Length);

			foreach (LoggingEvent log in memoryAppender.GetEvents())
			{
				Assert.AreEqual(typeof(IOperations).FullName, log.LoggerName);
				Assert.IsTrue(log.Properties.Contains("NDC"));
			}
		}

		[Test]
		public void CanCaptureRequestsAndResponsesUsingCustomFormat()
		{
			windsorContainer.Register(
				Component.For<LogMessageEndpointBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit)
					.Named("logMessageBehavior"),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.LogMessages("h")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(42, client.GetValueFromConstructor());
			Assert.AreEqual(4, memoryAppender.GetEvents().Length);

			foreach (LoggingEvent log in memoryAppender.GetEvents())
			{
				Assert.AreEqual(typeof(IOperations).FullName, log.LoggerName);
				Assert.IsTrue(log.Properties.Contains("NDC"));
			}
		}

		[Test]
		public void CanCaptureRequestsAndResponsesUsingGlobalFormatter()
		{
			windsorContainer.Register(
				Component.For<IFormatProvider>().ImplementedBy<HelloFormatter>(),
				Component.For<LogMessageEndpointBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit)
					.Named("logMessageBehavior"),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.LogMessages()
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(42, client.GetValueFromConstructor());
			Assert.AreEqual(4, memoryAppender.GetEvents().Length);

			int i = 0;
			foreach (LoggingEvent log in memoryAppender.GetEvents())
			{
				Assert.AreEqual(typeof(IOperations).FullName, log.LoggerName);
				Assert.IsTrue(log.Properties.Contains("NDC"));

				if ((++i % 2) == 0)
				{
					Assert.AreEqual("Hello", log.RenderedMessage);
				}
			}
		}

		[Test]
		public void CanCaptureRequestsAndResponsesUsingFormatExtension()
		{
			windsorContainer.Register(
				Component.For<HelloMessageFormat>(),
				Component.For<LogMessageEndpointBehavior>()
					.Named("logMessageBehavior"),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(42, client.GetValueFromConstructor());
			Assert.AreEqual(4, memoryAppender.GetEvents().Length);

			int i = 0;
			foreach (LoggingEvent log in memoryAppender.GetEvents())
			{
				Assert.AreEqual(typeof(IOperations).FullName, log.LoggerName);
				Assert.IsTrue(log.Properties.Contains("NDC"));

				if ((++i % 2) == 0)
				{
					Assert.AreEqual("Hello", log.RenderedMessage);
				}
			}
		}

		[Test]
		public void CanCaptureRequestsAndResponsesUsingExplicitFormatter()
		{
			windsorContainer.Register(
				Component.For<LogMessageEndpointBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit)
					.Named("logMessageBehavior"),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.LogMessages<HelloFormatter>()
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(42, client.GetValueFromConstructor());
			Assert.AreEqual(4, memoryAppender.GetEvents().Length);

			int i = 0;
			foreach (LoggingEvent log in memoryAppender.GetEvents())
			{
				Assert.AreEqual(typeof(IOperations).FullName, log.LoggerName);
				Assert.IsTrue(log.Properties.Contains("NDC"));

				if ((++i % 2) == 0)
				{
					Assert.AreEqual("Hello", log.RenderedMessage);
				}
			}
		}

		[Test]
		public void CanAddMessageHeader()
		{
			windsorContainer.Register(
				Component.For<MessageLifecycleBehavior>(),
				Component.For<LogMessageEndpointBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.AddExtensions(new AddOperationsHeader("MyHeader", "Hello"))
							.LogMessages()
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(42, client.GetValueFromConstructor());

			int i = 0;
			foreach (LoggingEvent log in memoryAppender.GetEvents())
			{
				if ((++i % 2) == 0)
				{
					Assert.IsTrue(log.RenderedMessage.Contains("<MyHeader>Hello</MyHeader>"));
				}
			}
		}

		[Test]
		public void CanModifyRequestAndResponseBody()
		{
			windsorContainer.Register(
				Component.For<MessageLifecycleBehavior>(),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.AddExtensions(new ReplaceOperationsResult("100"))
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(100, client.GetValueFromConstructor());
		}

		[Test]
		public void WillUseSameXmlDocumentForConsecutiveModifications()
		{
			StoreMessageBody start = new StoreMessageBody(MessageLifecycle.Requests);
			StoreMessageBody end = new StoreMessageBody(MessageLifecycle.Requests);

			windsorContainer.Register(
				Component.For<MessageLifecycleBehavior>(),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.AddExtensions(start, new ReplaceOperationsResult("100").ExecuteAt(1),
										   new ReplaceOperationsResult("200").ExecuteAt(2),
										   end)
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(200, client.GetValueFromConstructor());
			Assert.IsNotNull(start.Body);
			Assert.AreSame(start.Body, end.Body);
		}

		[Test]
		public void CanModifyRequestAndResponseBodyAndAddHeaders()
		{
			windsorContainer.Register(
				Component.For<MessageLifecycleBehavior>(),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.AddExtensions(new ReplaceOperationsResult("100"),
										   new AddOperationsHeader("MyHeader", "Hello"))
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(100, client.GetValueFromConstructor());

			int i = 0;
			foreach (LoggingEvent log in memoryAppender.GetEvents())
			{
				if ((++i % 2) == 0)
				{
					Assert.IsTrue(log.RenderedMessage.Contains("<MyHeader>Hello</MyHeader>"));
				}
			}
		}

		[Test]
		public void WillCreateNewXmlDocumentForNormalActions()
		{
			StoreMessageBody start = new StoreMessageBody(MessageLifecycle.Requests);
			StoreMessageBody end = new StoreMessageBody(MessageLifecycle.Requests);

			windsorContainer.Register(
				Component.For<MessageLifecycleBehavior>(),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
							.AddExtensions(start, new ReplaceOperationsResult("100").ExecuteAt(1),
										   new ReplaceOperationsResult("200").ExecuteAt(2),
										   new AddOperationsHeader("MyHeader", "Hello"),
										   end)
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			Assert.AreEqual(200, client.GetValueFromConstructor());
			Assert.IsNotNull(start.Body);
			Assert.IsNotNull(end.Body);
			Assert.AreNotSame(start.Body, end.Body);
		}

		[Test]
		public void WillReleaseAllExtensionsWhenUnregistered()
		{
			windsorContainer.Register(
				Component.For<MessageLifecycleBehavior>(),
				Component.For<IOperations>()
				.Named("operations")
				.AsWcfClient(new DefaultClientModel()
				{
					Endpoint = WcfEndpoint
						.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
						.At("net.tcp://localhost/Operations")
				})
			);
			windsorContainer.Resolve<IOperations>("operations");
			windsorContainer.Kernel.RemoveComponent("operations");
		}

		[Test]
		public void CanProxyChannelFactoriesForAsyncSupport()
		{
			var model = new DefaultClientModel()
				{
					Endpoint = WcfEndpoint
						.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
						.At("net.tcp://localhost/Operations")
				};
			var asyncBuilder = new AsynChannelFactoryBuilder<DefaultClientModel>(new ProxyGenerator());
			var channelFactory = asyncBuilder.CreateChannelFactory<ChannelFactory<IOperations>>(
				model, new NetTcpBinding(), "net.tcp://localhost/Operations");

			var client = channelFactory.CreateChannel();
			Assert.AreEqual(42, client.GetValueFromConstructor());
			((ICommunicationObject)client).Close();
		}

		[Test]
		public void CanCallChannelOperationsAsynchronously()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			var call = client.BeginWcfCall(p => p.GetValueFromConstructor());
			Assert.AreEqual(42, call.End());
		}

		[Test]
		public void CanCallBaseInterfaceAsynchronously()
		{
			windsorContainer.Register(
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations/Ex")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			var call = client.BeginWcfCall(p => p.GetValueFromConstructor());
			Assert.AreEqual(42, call.End());
		}

		[Test]
		public void CanCallChannelOperationsAsynchronouslyWithExplicitInterface()
		{
			windsorContainer.Register(
				Component.For<IOperationsAll>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperationsAll>("operations");
			var call = client.BeginGetValueFromConstructor(null, null);
			Assert.AreEqual(42, client.EndGetValueFromConstructor(call));
		}

		[Test]
		public void CanCallChannelOperationsAsynchronouslyUsingStandardAsyncPattern()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			var result = client.BeginWcfCall(p => p.GetValueFromConstructor());
			Assert.AreEqual(42, client.EndWcfCall<int>(result));
		}

		public void CanCallChannelOperationsWithRefArgumentsAsynchronously()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			int refValue = 0;
			var call = client.BeginWcfCall(p => p.GetValueFromConstructorAsRef(ref refValue));
			Assert.AreEqual(42, call.End(out refValue));
			Assert.AreEqual(42, refValue);
		}

		[Test]
		public void CanCallChannelOperationsWithOutAndRefArgumentsAsynchronously()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			int refValue = 0, outValue;
			var call = client.BeginWcfCall(p => p.GetValueFromConstructorAsRefAndOut(ref refValue, out outValue));
			Assert.AreEqual(42, call.End(out refValue, out outValue));
			Assert.AreEqual(42, refValue);
			Assert.AreEqual(42, outValue);
		}

		[Test]
		public void CanCallChannelOperationsAsynchronouslyUsingServiceEndpoint()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.FromEndpoint(new ServiceEndpoint(
								ContractDescription.GetContract(typeof(IOperations)),
								new NetTcpBinding { PortSharingEnabled = true },
								new EndpointAddress("net.tcp://localhost/Operations")
								))
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			var call = client.BeginWcfCall(p => p.GetValueFromConstructor());
			Assert.AreEqual(42, call.End());
		}

		[Test]
		public void WillCallBackWhenAsynchronousOperationCompletes()
		{
			windsorContainer.Register(
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperations>("operations");
			var result = client.BeginWcfCall(p => p.GetValueFromConstructor(),
				call => Assert.AreEqual(42, call.End()), null);
			result.AsyncWaitHandle.WaitOne(5000);
		}

		[Test]
		public void WillCallBackResultWhenAsynchronousOperationCompletes()
		{
			using (IWindsorContainer localContainer = new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<IOperations>()
						.ImplementedBy<Operations>()
						.DependsOn(new { number = 22 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None))
								.At("net.pipe://localhost/Operations")
								)
						),
					Component.For<IOperations>()
						.Named("operations")
						.AsWcfClient(new DefaultClientModel()
						{
							Endpoint = WcfEndpoint
								.BoundTo(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None))
								.At("net.pipe://localhost/Operations")
						})
					))
			{
				var client = localContainer.Resolve<IOperations>("operations");
				var result = client.BeginWcfCall(p => p.GetValueFromConstructor(),
					(IAsyncResult ar) => Assert.AreEqual(22, client.EndWcfCall<int>(ar)), null);
				Assert.True(result.AsyncWaitHandle.WaitOne(5000));
			}
		}

		[Test]
		public void CanCallChannelOperationsAsynchronouslyOnAsyncService()
		{
			using (IWindsorContainer localContainer = new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<IAsyncOperations>()
						.ImplementedBy<AsyncOperations>()
						.DependsOn(new { number = 22 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None))
								.At("net.pipe://localhost/Operations")
								)
						),
					Component.For<IOperations>()
						.Named("operations")
						.AsWcfClient(new DefaultClientModel()
						{
							Endpoint = WcfEndpoint
								.BoundTo(new NetNamedPipeBinding(NetNamedPipeSecurityMode.None))
								.At("net.pipe://localhost/Operations")
						})
					))
			{
				var client = localContainer.Resolve<IOperations>("operations");
				var result = client.BeginWcfCall(p => p.GetValueFromConstructor(),
					(IAsyncResult ar) => Assert.AreEqual(22, client.EndWcfCall<int>(ar)), null);
				Assert.True(result.AsyncWaitHandle.WaitOne(5000));
			}
		}

		[Test]
		public void CanCallChannelOperationsAsynchronouslyWithExplicitAsyncPattern()
		{
			windsorContainer.Register(
				Component.For<IOperationsAll>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
				);

			var client = windsorContainer.Resolve<IOperationsAll>("operations");
			var result = client.BeginWcfCall(p => p.GetValueFromConstructor(),
				(IAsyncResult ar) => Assert.AreEqual(42, client.EndWcfCall<int>(ar)), null);
			Assert.True(result.AsyncWaitHandle.WaitOne(5000));
		}

		[Test]
		public void WillApplyCustomInterceptorsWhenCallingMethodsAsynchronously()
		{
			windsorContainer.Register(
				Component.For<TraceInterceptor>(),
				Component.For<IOperations>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel()
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations")
					})
					.Interceptors(InterceptorReference.ForType<TraceInterceptor>()).Anywhere
				);

			TraceInterceptor.Reset();
			Assert.IsNull(TraceInterceptor.MethodCalled);

			var client = windsorContainer.Resolve<IOperations>("operations");
			var call = client.BeginWcfCall(p => p.GetValueFromConstructor());
			Assert.AreEqual(42, call.End());
			Assert.AreEqual("GetValueFromConstructor", TraceInterceptor.MethodCalled.Name);
		}

		[Test, ExpectedException(typeof(EndpointNotFoundException))]
		public void ThrowsEndPointNotFoundException()
		{
			Func<IWindsorContainer> createLocalContainer = () =>
				new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations1"))
						)
					);

			windsorContainer.Register(
				Component.For<IOperationsEx>()
					.Named("operations")
					.AsWcfClient(new DefaultClientModel
					{
						Endpoint = WcfEndpoint
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
					})
				);

			using (createLocalContainer())
			{
				var client = windsorContainer.Resolve<IOperationsEx>("operations");
				client.Backup(new Dictionary<string, object>());
			}
		}

#if DOTNET40
		[Test]
		public void CanDiscoverServiceEndpointAndInferBinding()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2"))
						.Discoverable()
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().InferBinding())
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanDiscoverServiceEndpointFromMetadata()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding(SecurityMode.Transport, true)
							{
								PortSharingEnabled = true
							})
							.At("net.tcp://localhost/Operations2"))
						.Discoverable()
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover())
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanDiscoverServiceEndpointFromMetadataWithPreference()
		{
			var preferenceMade = false;

			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding(SecurityMode.Transport, true)
							{
								PortSharingEnabled = true
							})
							.At("net.tcp://localhost/Operations2"))
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding(SecurityMode.Transport, true)
							{
								PortSharingEnabled = true
							})
							.At("net.tcp://localhost/Operations2a"))
						.Discoverable()
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().Limit(2).PreferEndpoint(endpoints =>
						{
							preferenceMade = true;
							return endpoints[0];
						}))
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
					Assert.IsTrue(preferenceMade);
				}
			}
		}

		[Test, ExpectedException(typeof(EndpointNotFoundException))]
		public void WillNotDiscoverServiceEndpointIfScopesDontMatch()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2"))
						.Discoverable(discover => discover.InScope("urn:castle:wcf"))
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover()
							.InScope("urn:castle:rocks")
							.Span(TimeSpan.FromSeconds(2)))
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanDiscoverServiceEndpointAndInferBindingWithScope()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2"))
						.Discoverable(discover => discover.InScope("urn:castle:wcf"))
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().InScope("urn:castle:wcf"))
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanDiscoverServiceEndpointAndInferBindingWithEndpointScope()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							.InScope("urn:castle:wcf"))
						.Discoverable()
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().InferBinding().InScope("urn:castle:wcf"))
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanDiscoverServiceEndpointAndInferBindingWithCombindedScopes()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2")
							.InScope("urn:castle:wcf"))
						.Discoverable(discover => discover.InScope("urn:castle:ioc"))
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().InferBinding()
						.InScope("urn:castle:wcf", "urn:castle:ioc"))
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanDiscoverServiceEndpointFromMetadataWithScope()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding(SecurityMode.Transport, true)
							{
								PortSharingEnabled = true
							})
							.At("net.tcp://localhost/Operations2"))
						.Discoverable(discover => discover.InScope("urn:castle:wcf"))
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().InScope("urn:castle:wcf"))
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanAddAdditionalDiscoveryMetadata()
		{
			using (var container = new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding(SecurityMode.Transport, true)
							{
								PortSharingEnabled = true
							})
							.At("net.tcp://localhost/Operations2"))
						.ProvideMetadata<WcfMetadataProvider<ServiceHostBase>>()
						.Discoverable()
				)))
			{
				var metadata = new WcfMetadataProvider<ServiceHostBase>();
				metadata.Scopes.Add(new Uri("urn:castle:wcf"));
				container.Register(Component.For<WcfMetadataProvider<ServiceHostBase>>().Instance(metadata));

				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().InScope("urn:castle:wcf"))
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					Assert.AreEqual(28, client.GetValueFromConstructor());
				}
			}
		}

		[Test]
		public void CanAccessDiscoverServiceEndpointMetadata()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 28 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations2"))
						.Discoverable()
				)))
			{
				using (var clientContainer = new WindsorContainer()
					.AddFacility<WcfFacility>()
					.Register(Component.For<IOperations>()
						.AsWcfClient(WcfEndpoint.Discover().InferBinding())
					))
				{
					var client = clientContainer.Resolve<IOperations>();
					var metadata = ((IContextChannel)client).Extensions.Find<DiscoveredEndpointMetadata>();
					Assert.IsNotNull(metadata);
					Assert.IsNotNull(metadata.Metadata);
				}
			}
		}
#endif
		protected void RegisterLoggingFacility(IWindsorContainer container)
		{
			var facNode = new MutableConfiguration("facility");
			facNode.Attributes["id"] = "logging";
			facNode.Attributes["loggingApi"] = "ExtendedLog4net";
			facNode.Attributes["configFile"] = "";
			container.Kernel.ConfigurationStore.AddFacilityConfiguration("logging", facNode);
			container.AddFacility("logging", new LoggingFacility());

			memoryAppender = new MemoryAppender();
			BasicConfigurator.Configure(memoryAppender);
		}

		private static string xmlConfiguration = @"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
	<facilities>
		<facility id='wcf' 
				  type='Castle.Facilities.WcfIntegration.WcfFacility,
				        Castle.Facilities.WcfIntegration' />
	</facilities>

	<components>
		<component id='usingWindsor'
			       type='Castle.Facilities.WcfIntegration.Demo.IAmUsingWindsor, 
				         Castle.Facilities.WcfIntegration.Demo'
			       wcfEndpointConfiguration='WSHttpBinding_IAmUsingWindsor'>
		</component>
	</components>
</configuration>";
	}

	public class EmptyInterceptor : MarshalByRefObject, IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
		}
	}

}
