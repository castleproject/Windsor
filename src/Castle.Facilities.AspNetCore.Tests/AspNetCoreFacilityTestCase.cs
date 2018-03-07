// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNetCore.Tests
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Builder.Internal;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Razor.TagHelpers;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

	using NUnit.Framework;

	[TestFixture]
	public class AspNetCoreFacilityTestCase
	{
		[TearDown]
		public void TearDown()
		{
			scope.Dispose();
			container.Dispose();
		}

		private IDisposable scope;
		private WindsorContainer container;

		[TestCase(true)]
		[TestCase(false)]
		public void Should_be_able_to_resolve_controller_from_container(bool useEntryAssembly)
		{
			SetUp(useEntryAssembly);
			Assert.DoesNotThrow(() => container.Resolve<AnyController>());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_be_able_to_resolve_tag_helper_from_container(bool useEntryAssembly)
		{
			SetUp(useEntryAssembly);
			Assert.DoesNotThrow(() => container.Resolve<AnyTagHelper>());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_be_able_to_resolve_view_component_from_container(bool useEntryAssembly)
		{
			SetUp(useEntryAssembly);
			Assert.DoesNotThrow(() => container.Resolve<AnyViewComponent>());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Should_throw_if_framework_components_registered_in_Windsor_to_prevent_torn_lifestyles_once_configuration_is_validated(bool useEntryAssembly)
		{
			SetUp(useEntryAssembly);
			Assert.Throws<Exception>(() =>
			{
				container.Register(Component.For<FakeFrameworkComponent>());
				container.AssertNoAspNetCoreRegistrations();
			});
		}

		private static ServiceCollection BuildServiceCollection()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
			serviceCollection.AddTransient<FakeFrameworkComponentEnumerable>();
			serviceCollection.AddTransient<IFakeFrameworkComponent, FakeFrameworkComponent>();
			serviceCollection.AddTransient<IFakeFrameworkComponent<FakeFrameworkOptions>, FakeFrameworkComponent<FakeFrameworkOptions>>();
			serviceCollection.AddTransient(typeof(IFakeFrameworkComponent<>), typeof(FakeFrameworkComponent<>));
			serviceCollection.AddTransient<IList<IFakeFrameworkComponentEnumerable>>((sp) => new List<IFakeFrameworkComponentEnumerable>()
			{
				sp.GetRequiredService<FakeFrameworkComponentEnumerable>(),
				sp.GetRequiredService<FakeFrameworkComponentEnumerable>(),
				sp.GetRequiredService<FakeFrameworkComponentEnumerable>(),
			});
			return serviceCollection;
		}

		private void BuildWindsorContainer(ServiceCollection serviceCollection)
		{
			container = new WindsorContainer();
			serviceCollection.AddCastleWindsor(container);
			container.Register(Component.For<AnyService>());
		}

		private void BuildApplicationBuilder(ServiceCollection serviceCollection, bool useEntryAssembly)
		{
			var applicationBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());
			if (useEntryAssembly)
			{
				applicationBuilder.UseCastleWindsor(container);
			}
			applicationBuilder.UseCastleWindsor<AspNetCoreFacilityTestCase>(container);
		}

		public class AnyService
		{
		}

		public class AnyController : Controller
		{
			private readonly IFakeFrameworkComponent<FakeFrameworkOptions> closedGeneric;
			private readonly IFakeFrameworkComponent component;
			private readonly IList<IFakeFrameworkComponentEnumerable> components;
			private readonly ILogger logger;
			private readonly IFakeFrameworkComponent<string> openGeneric;
			private readonly AnyService service;

			public AnyController(
				AnyService service,
				IFakeFrameworkComponent component,
				IList<IFakeFrameworkComponentEnumerable> components,
				IFakeFrameworkComponent<string> openGeneric,
				IFakeFrameworkComponent<FakeFrameworkOptions> closedGeneric,
				ILogger logger)
			{
				this.service = service ?? throw new ArgumentNullException(nameof(service));
				this.component = component ?? throw new ArgumentNullException(nameof(component));
				this.components = components ?? throw new ArgumentNullException(nameof(components));
				this.closedGeneric = closedGeneric ?? throw new ArgumentNullException(nameof(closedGeneric));
				this.openGeneric = openGeneric ?? throw new ArgumentNullException(nameof(openGeneric));
				this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			}
		}

		public class AnyTagHelper : TagHelper
		{
			private readonly IFakeFrameworkComponent component;
			private readonly AnyService service;

			public AnyTagHelper(AnyService service, IFakeFrameworkComponent component)
			{
				this.service = service ?? throw new ArgumentNullException(nameof(service));
				this.component = component ?? throw new ArgumentNullException(nameof(component));
			}
		}

		public class AnyViewComponent : ViewComponent
		{
			private readonly IFakeFrameworkComponent component;
			private readonly AnyService service;

			public AnyViewComponent(AnyService service, IFakeFrameworkComponent component)
			{
				this.service = service ?? throw new ArgumentNullException(nameof(service));
				this.component = component ?? throw new ArgumentNullException(nameof(component));
			}
		}

		private void SetUp(bool useEntryAssembly)
		{
			var serviceCollection = BuildServiceCollection();
			BuildWindsorContainer(serviceCollection);
			BuildApplicationBuilder(serviceCollection, useEntryAssembly);
			scope = container.BeginScope();
		}
	}
}

namespace Microsoft.AspNetCore
{
	public interface IFakeFrameworkComponent { }
	public class FakeFrameworkComponent : IFakeFrameworkComponent { }

	public interface IFakeFrameworkComponent<T> { }
	public class FakeFrameworkComponent<T> : IFakeFrameworkComponent<T> { }

	public class FakeFrameworkOptions { }
	public interface IFakeFrameworkComponentEnumerable { }

	public class FakeFrameworkComponentEnumerable : IFakeFrameworkComponentEnumerable { }
}