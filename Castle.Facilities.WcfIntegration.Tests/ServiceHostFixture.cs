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

namespace Castle.Facilities.WcfIntegration.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Description;
	using Castle.Core.Configuration;
	using Castle.Core.Resource;
	using Castle.Facilities.Logging;
	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Demo;
	using Castle.Facilities.WcfIntegration.Tests.Behaviors;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Installer;
	using log4net.Appender;
	using log4net.Config;
	using NUnit.Framework;

	[TestFixture]
	public class ServiceHostFixture
	{
		private MemoryAppender memoryAppender;

		[Test]
		public void CanCreateServiceHostAndOpenHost()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<IOperations>()
					.ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations"))
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
			}
		}

		[Test, Explicit]
		public void CanCreateServiceHostPerCallAndOpenHost()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<IServiceBehavior>()
					.Instance(new ServiceBehaviorAttribute()
					{
						InstanceContextMode = InstanceContextMode.PerCall,
						ConcurrencyMode = ConcurrencyMode.Multiple
					}),
					Component.For<IOperations>()
					.ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations"))
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true },
					new EndpointAddress("net.tcp://localhost/Operations"));
				((IClientChannel)client).Open();

				for (int i = 0; i < 10; i++)
				{
					new System.Threading.Thread(() =>
					{
						int refValue = 0, outValue;
						Assert.AreEqual(42, client.GetValueFromConstructorAsRefAndOut(ref refValue, out outValue));
						//var result = client.BeginGetValueFromConstructorAsRefAndOut(ref refValue, null, null);
						//Assert.AreEqual(42, client.EndGetValueFromConstructorAsRefAndOut(ref refValue, out outValue, result));
					}).Start();
				}
				System.Threading.Thread.CurrentThread.Join();
				//((ICommunicationObject)client).Close();
			}
		}

		[Test]
		public void CanCreateServiceHostAndOpenHostUsingDefaultBinding()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => 
					{ 
						f.DefaultBinding = new NetTcpBinding { PortSharingEnabled = true };
						f.CloseTimeout = TimeSpan.Zero;
					}
				)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.ForContract<IOperations>()
							.At("net.tcp://localhost/Operations")
						)
				)))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
			}
		}

		[Test]
		public void CanCreateServiceHostAndOpenHostFromConfiguration()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<UsingWindsor>()
					.DependsOn(new { number = 42 })
					.AsWcfService()
				))
			{
				var client = ChannelFactory<IAmUsingWindsor>.CreateChannel(
					new BasicHttpBinding(), new EndpointAddress("http://localhost:27198/UsingWindsor.svc"));
				Assert.AreEqual(42, client.GetValueFromWindsorConfig());
			}
		}

		[Test]
		public void CanCreateServiceHostAndOpenHostWithMultipleEndpoints()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.ForContract<IOperations>()
							.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations"),
						WcfEndpoint.ForContract<IOperationsEx>()
							.BoundTo(new BasicHttpBinding())
							.At("http://localhost:27198/UsingWindsor.svc")
						)
				)))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());

				var clientEx = ChannelFactory<IOperationsEx>.CreateChannel(
					new BasicHttpBinding(), new EndpointAddress("http://localhost:27198/UsingWindsor.svc"));
				clientEx.Backup(new Dictionary<string, object>());
			}
		}

		[Test]
		public void CanCreateServiceHostAndOpenHostWithRelativeEndpoints()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
						.AddBaseAddresses(
							"net.tcp://localhost/Operations",
							"http://localhost:27198/UsingWindsor.svc")
						.AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true }),
							WcfEndpoint.ForContract<IOperationsEx>()
								.BoundTo(new BasicHttpBinding())
								.At("Extended")
							)
					)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());

				var clientEx = ChannelFactory<IOperationsEx>.CreateChannel(
					new BasicHttpBinding(), new EndpointAddress("http://localhost:27198/UsingWindsor.svc/Extended"));
				clientEx.Backup(new Dictionary<string, object>());
			}
		}

		[Test]
		public void CanCreateServiceHostAndOpenHostWithListenAddress()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<IOperations>()
					.ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("urn:castle:operations")
							.Via("net.tcp://localhost/Operations")
							)
				)))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("urn:castle:operations"),
					new Uri("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
			}
		}

		[Test]
		public void CanCreateServiceHostAndOpenHostFromXmlConfiguration()
		{
			using (new WindsorContainer()
					.Install(Configuration.FromXml(new StaticContentResource(xmlConfiguration))))
			{
				var client = ChannelFactory<IAmUsingWindsor>.CreateChannel(
					new BasicHttpBinding(), new EndpointAddress("http://localhost:27198/UsingWindsor.svc"));
				Assert.AreEqual(42, client.GetValueFromWindsorConfig());
			}
		}

		[Test]
		public void CanCreateServiceHostAndOpenHostWithMultipleServiceModels()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<IOperations>()
					.ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(
						new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations")
								),
						new DefaultServiceModel()
							.AddBaseAddresses(
								"http://localhost:27198/UsingWindsor.svc")
							.AddEndpoints(
								WcfEndpoint.ForContract<IOperationsEx>()
									.BoundTo(new BasicHttpBinding())
									.At("Extended")
								)
							)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());

				var clientEx = ChannelFactory<IOperationsEx>.CreateChannel(
					new BasicHttpBinding(), new EndpointAddress("http://localhost:27198/UsingWindsor.svc/Extended"));
				clientEx.Backup(new Dictionary<string, object>());
			}
		}

		[Test]
		public void WillApplyServiceScopedBehaviors()
		{
			CallCountServiceBehavior.CallCount = 0;
			Assert.IsFalse(UnitOfWork.initialized, "Should be false before starting");

			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<CallCountServiceBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Clients),
					Component.For<UnitOfworkEndPointBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Services),
					Component.For<IOperations>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations"))
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				bool unitOfWorkIsInitialized_DuringCall = client.UnitOfWorkIsInitialized();
				Assert.IsTrue(unitOfWorkIsInitialized_DuringCall);
				Assert.IsFalse(UnitOfWork.initialized, "Should be false after call");
				Assert.AreEqual(0, CallCountServiceBehavior.CallCount);
			}
		}

		[Test, Explicit("It doesn't not working 3.5. I guess that i shouldn't work, but need review.")]
		public void WillApplyServiceScopedBehaviorsToDefaultEndpoint()
		{
			CallCountServiceBehavior.CallCount = 0;
			Assert.IsFalse(UnitOfWork.initialized, "Should be false before starting");

			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<CallCountServiceBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Services),
					Component.For<UnitOfworkEndPointBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Services),
					Component.For<IOperations>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddBaseAddresses(
						"net.tcp://localhost/Operations"))
					)
				)
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				bool unitOfWorkIsInitialized_DuringCall = client.UnitOfWorkIsInitialized();
				Assert.IsTrue(unitOfWorkIsInitialized_DuringCall);
				Assert.IsFalse(UnitOfWork.initialized, "Should be false after call");
				Assert.AreEqual(1, CallCountServiceBehavior.CallCount);
			}
		}

		[Test]
		public void WillApplyServiceScopedBehaviorsToMultipleEndpoints()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<DummyContractBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Services),
					Component.For<IOperations>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"),
							WcfEndpoint.BoundTo(new BasicHttpBinding())
								.At("http://localhost/Operations")
							)
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
			}
		}

		[Test]
		public void WillApplyExplcitScopedKeyBehaviors()
		{
			CallCountServiceBehavior.CallCount = 0;

			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<CallCountServiceBehavior>()
						.Named("callcounts")
						.Attribute("scope").Eq(WcfExtensionScope.Explicit),
					Component.For<IOperations>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"))
						.AddEndpoints(
							WcfEndpoint.BoundTo(new BasicHttpBinding())
								.At("http://localhost/Operations"))
						.AddExtensions("callcounts")
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
				Assert.AreEqual(1, CallCountServiceBehavior.CallCount);
			}
		}

		[Test]
		public void WillApplyExplcitScopedServiceBehaviors()
		{
			CallCountServiceBehavior.CallCount = 0;

			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<CallCountServiceBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Explicit),
					Component.For<IOperations>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"))
						.AddExtensions(typeof(CallCountServiceBehavior))
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
				Assert.AreEqual(1, CallCountServiceBehavior.CallCount);
			}
		}

		[Test]
		public void WillApplyInstanceBehaviors()
		{
			CallCountServiceBehavior.CallCount = 0;

			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<IOperations>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
						.AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"))
						.AddExtensions(new CallCountServiceBehavior(),
									   new UnitOfworkEndPointBehavior())
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				bool unitOfWorkIsInitialized_DuringCall = client.UnitOfWorkIsInitialized();
				Assert.IsTrue(unitOfWorkIsInitialized_DuringCall);
				Assert.IsFalse(UnitOfWork.initialized, "Should be false after call");
				Assert.AreEqual(1, CallCountServiceBehavior.CallCount);
			}
		}

		[Test]
		public void WillApplyErrorHandlersToServices()
		{
			using (RegisterLoggingFacility(new WindsorContainer())
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<ErrorLogger>(),
					Component.For<IOperationsEx>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations"))
						)
				))
			{
				var client = ChannelFactory<IOperationsEx>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));

				try
				{
					client.ThrowException();
					Assert.Fail("Should have raised an exception");
				}
				catch
				{
					foreach (var log in memoryAppender.GetEvents())
					{
						Assert.AreEqual("An error has occurred", log.RenderedMessage);
						Assert.AreEqual("Oh No!", log.ExceptionObject.Message);
					}
				}
			}
		}

		[Test]
		public void WillNotApplyErrorHandlersToServicesIfExplicit()
		{
			using (RegisterLoggingFacility(new WindsorContainer())
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<ErrorLogger>()
						.Attribute("scope").Eq(WcfExtensionScope.Explicit),
					Component.For<IOperationsEx>().ImplementedBy<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							.At("net.tcp://localhost/Operations"))
						)
				))
			{
				var client = ChannelFactory<IOperationsEx>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));

				try
				{
					client.ThrowException();
					Assert.Fail("Should have raised an exception");
				}
				catch
				{
					CollectionAssert.IsEmpty(memoryAppender.GetEvents());
				}
			}
		}

		[Test]
		public void WillApplyErrorHandlersToServicesExplicitly()
		{
			using (RegisterLoggingFacility(new WindsorContainer())
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
					.Register(
						Component.For<ErrorLogger>()
							.Attribute("scope").Eq(WcfExtensionScope.Explicit),
						Component.For<IOperationsEx>().ImplementedBy<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"))
								.AddExtensions(typeof(ErrorLogger))
							)
					))
			{
				var client = ChannelFactory<IOperationsEx>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));

				try
				{
					client.ThrowException();
					Assert.Fail("Should have raised an exception");
				}
				catch
				{
					foreach (var log in memoryAppender.GetEvents())
					{
						Assert.AreEqual("An error has occurred", log.RenderedMessage);
						Assert.AreEqual("Oh No!", log.ExceptionObject.Message);
					}
				}
			}
		}

		[Test]
		public void WillApplyErrorHandlersToEndpointsExplicitly()
		{
			using (RegisterLoggingFacility(new WindsorContainer())
					.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
					.Register(
						Component.For<ErrorLogger>()
							.Attribute("scope").Eq(WcfExtensionScope.Explicit),
						Component.For<IOperationsEx>().ImplementedBy<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations")
								.AddExtensions(typeof(ErrorLogger)))
							)
					))
			{
				var client = ChannelFactory<IOperationsEx>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));

				try
				{
					client.ThrowException();
					Assert.Fail("Should have raised an exception");
				}
				catch
				{
					foreach (var log in memoryAppender.GetEvents())
					{
						Assert.AreEqual("An error has occurred", log.RenderedMessage);
						Assert.AreEqual("Oh No!", log.ExceptionObject.Message);
					}
				}
			}
		}

		[Test]
		public void CanCaptureRequestsAndResponsesAtEndpointLevel()
		{
			using (RegisterLoggingFacility(new WindsorContainer())
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<LogMessageEndpointBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Explicit)
						.Named("logMessageBehavior"),
					Component.For<IOperations>()
						.ImplementedBy<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations")
								.LogMessages()
							))
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
				Assert.AreEqual(4, memoryAppender.GetEvents().Length);

				foreach (var log in memoryAppender.GetEvents())
				{
					Assert.AreEqual(typeof(Operations).FullName, log.LoggerName);
					Assert.IsTrue(log.Properties.Contains("NDC"));
				}
			}
		}

		[Test]
		public void CanCaptureRequestsAndResponsesAtServiceLevel()
		{
			using (RegisterLoggingFacility(new WindsorContainer())
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<LogMessageEndpointBehavior>()
						.Attribute("scope").Eq(WcfExtensionScope.Explicit)
						.Named("logMessageBehavior"),
					Component.For<IOperations>()
						.ImplementedBy<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"))
							.LogMessages()
							)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());
				Assert.AreEqual(4, memoryAppender.GetEvents().Length);
			}
		}

		[Test]
		public void CanModifyRequestsAndResponses()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<MessageLifecycleBehavior>(),
					Component.For<IOperations>()
						.ImplementedBy<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations")
								.AddExtensions(new ReplaceOperationsResult("100")))
							)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, 
					new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(100, client.GetValueFromConstructor());
			}
		}

		[Test]
		public void CanGiveFriendlyErrorMessageForUunresolvedServiceDependenciesIfOpenEagerly()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
					Component.For<IServiceBehavior>()
						.Instance(new ServiceDebugBehavior()
						{
							IncludeExceptionDetailInFaults = true
						}),
					Component.For<IOperations>()
						.ImplementedBy<Operations>()
						.AsWcfService(new DefaultServiceModel()
							.OpenEagerly()
							.AddEndpoints(WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"))
						)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, 
					new EndpointAddress("net.tcp://localhost/Operations"));

				try
				{
					Assert.AreEqual(42, client.GetValueFromConstructor());
				}
				catch (FaultException<ExceptionDetail>)
				{
				}
				catch (Exception ex)
				{
					Assert.Fail("Expected a {0}, but got {1}", typeof(FaultException<ExceptionDetail>).FullName, ex.GetType().FullName);
				}
			}
		}

		[Test]
		public void CanCreateServiceHostWithAspNetCompatibility()
		{
			var captureServiceHost = new CaptureServiceHost();

			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => 
					{
						f.CloseTimeout = TimeSpan.Zero;
						f.Services.AspNetCompatibility = AspNetCompatibilityRequirementsMode.Allowed;
					})
				.Register(
					Component.For<CaptureServiceHost>().Instance(captureServiceHost),
					Component.For<IOperations>()
						.ImplementedBy<Operations>()
						.DependsOn(new { number = 42 })
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
							WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
								.At("net.tcp://localhost/Operations"))
							)
				))
			{
				var client = ChannelFactory<IOperations>.CreateChannel(
					new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
				Assert.AreEqual(42, client.GetValueFromConstructor());

				AspNetCompatibilityRequirementsAttribute aspNetCompat =
					captureServiceHost.ServiceHost.Description.Behaviors.Find<AspNetCompatibilityRequirementsAttribute>();
				Assert.IsNotNull(aspNetCompat);
				Assert.AreEqual(AspNetCompatibilityRequirementsMode.Allowed, aspNetCompat.RequirementsMode);
			}
		}

		[Test]
		public void CanPubishMEXEndpointsUsingDefaults()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
						.AddBaseAddresses(
							"net.tcp://localhost/Operations",
							"http://localhost:27198/UsingWindsor.svc")
						.AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							)
						.PublishMetadata(mex => mex.EnableHttpGet())
					)
				))
			{
				var tcpMextClient = new MetadataExchangeClient(new EndpointAddress("net.tcp://localhost/Operations/mex"));
				var tcpMetadata = tcpMextClient.GetMetadata();
				Assert.IsNotNull(tcpMetadata);

				var httpMextClient = new MetadataExchangeClient(new EndpointAddress("http://localhost:27198/UsingWindsor.svc?wsdl"));
				var httpMetadata = httpMextClient.GetMetadata();
				Assert.IsNotNull(httpMextClient);
			}
		}

		[Test]
		public void CanPubishMEXEndpointsUsingCustomAddress()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
						.AddBaseAddresses(
							"net.tcp://localhost/Operations",
							"http://localhost:27198/UsingWindsor.svc")
						.AddEndpoints(
							WcfEndpoint.ForContract<IOperations>()
								.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
							)
						.PublishMetadata(mex => mex.EnableHttpGet().AtAddress("tellMeAboutYourSelf"))
					)
				))
			{
				var tcpMextClient = new MetadataExchangeClient(new EndpointAddress("net.tcp://localhost/Operations/tellMeAboutYourSelf"));
				var tcpMetadata = tcpMextClient.GetMetadata();
				Assert.IsNotNull(tcpMetadata);

				var httpMextClient = new MetadataExchangeClient(new EndpointAddress("http://localhost:27198/UsingWindsor.svc?wsdl"));
				var httpMetadata = httpMextClient.GetMetadata();
				Assert.IsNotNull(httpMetadata);
			}
		}

		[Test]
		public void CanPubishMEXEndpointsWithoutBaseAddresses()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f =>
				{
					f.DefaultBinding = new NetTcpBinding { PortSharingEnabled = true };
					f.CloseTimeout = TimeSpan.Zero;
				}
				)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.ForContract<IOperations>()
							.At("net.tcp://localhost/Operations"))
					.PublishMetadata()
				)))
			{
				var tcpMextClient = new MetadataExchangeClient(new EndpointAddress("net.tcp://localhost/Operations/mex"));
				var tcpMetadata = tcpMextClient.GetMetadata();
				Assert.IsNotNull(tcpMetadata);			
			}
		}

		[Test]
		public void CanPubishMEXEndpointsWithoutBaseAddressesUsingCustomAddress()
		{
			using (new WindsorContainer()
				.AddFacility<WcfFacility>(f =>
				{
					f.DefaultBinding = new NetTcpBinding { PortSharingEnabled = true };
					f.CloseTimeout = TimeSpan.Zero;
				}
				)
				.Register(Component.For<Operations>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
						WcfEndpoint.ForContract<IOperations>()
							.At("net.tcp://localhost/Operations"))
					.PublishMetadata(mex => mex.AtAddress("tellMeAboutYourSelf"))
				)))
			{
				var tcpMextClient = new MetadataExchangeClient(new EndpointAddress("net.tcp://localhost/Operations/tellMeAboutYourSelf"));
				var tcpMetadata = tcpMextClient.GetMetadata();
				Assert.IsNotNull(tcpMetadata);
			}
		}

		[Test, Ignore("Does not work with open-generics")]
		public void CanOpenServiceHostsWithServicesDependingOnOpenGenerics()
		{
			using (var container = new WindsorContainer()
				.AddFacility<WcfFacility>(f =>
				{
					//f.Services.OpenServiceHostsEagerly = true;
					f.DefaultBinding = new NetTcpBinding { PortSharingEnabled = true };
					f.CloseTimeout = TimeSpan.Zero;
				})
				.Register(
					Component.For(typeof(IDecorator<>)).ImplementedBy(typeof(Decorator<>)),
					Component.For<IServiceGenericDependency>().ImplementedBy<ServiceGenericDependency>().LifeStyle.Transient
						.AsWcfService(new DefaultServiceModel().AddEndpoints(
								WcfEndpoint.BoundTo(new NetTcpBinding())
									.At("net.tcp://localhost/Operations")
								)
						),
					Component.For<IServiceNoDependencies>().UsingFactoryMethod(() => new ServiceNoDependencies())
				))
			{
				var client = ChannelFactory<IServiceGenericDependency>.CreateChannel(
					 new NetTcpBinding(), new EndpointAddress("net.tcp://localhost/Operations"));

				client.DoSomething();
			}
		}

		protected IWindsorContainer RegisterLoggingFacility(IWindsorContainer container)
		{
			var facNode = new MutableConfiguration("facility");
			facNode.Attributes["id"] = "logging";
			facNode.Attributes["loggingApi"] = "ExtendedLog4net";
			facNode.Attributes["configFile"] = "";
			container.Kernel.ConfigurationStore.AddFacilityConfiguration("logging", facNode);
			container.AddFacility("logging", new LoggingFacility());

			memoryAppender = new MemoryAppender();
			BasicConfigurator.Configure(memoryAppender);
			return container;
		}

		private static string xmlConfiguration = @"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
	<facilities>
		<facility id='wcf' 
				  type='Castle.Facilities.WcfIntegration.WcfFacility,
				        Castle.Facilities.WcfIntegration' />
	</facilities>

	<components>
		<component id='usingwindsor_svc'
			       service='Castle.Facilities.WcfIntegration.Demo.IAmUsingWindsor, 
				            Castle.Facilities.WcfIntegration.Demo'
			       type='Castle.Facilities.WcfIntegration.Demo.UsingWindsor, 
				         Castle.Facilities.WcfIntegration.Demo'
			       wcfServiceHost='true'>
			<parameters>
				<number>42</number>
			</parameters>
		</component>
	</components>
</configuration>";


		interface IServiceNoDependencies
		{
		}

		class ServiceNoDependencies : IServiceNoDependencies
		{
		}

		interface IDecorator<T>
		where T : class
		{
		}

		class Decorator<T> : IDecorator<T>
			where T : class
		{
			public Decorator(T arg)
			{
			}
		}

		[ServiceContract]
		interface IServiceGenericDependency
		{
			[OperationContract]
			void DoSomething();
		}

		class ServiceGenericDependency : IServiceGenericDependency
		{
			public ServiceGenericDependency(IDecorator<IServiceNoDependencies> arg2)
			{
			}

			public void DoSomething()
			{

			}
		}
	}
}
