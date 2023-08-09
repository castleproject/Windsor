using Castle.MicroKernel.Registration;
using Castle.Windsor.Extensions.DependencyInjection.Extensions;
using Castle.Windsor.Extensions.DependencyInjection.Tests.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	public class ResolveFromThreadpoolUnsafe
	{
		#region Singleton

		[Fact]
		public async Task Can_Resolve_LifestyleSingleton_From_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleSingleton_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = container.Resolve<IUserService>(nameof(UserService));
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleNetStatic_From_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifeStyle.NetStatic()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleNetStatic_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifeStyle.NetStatic()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = container.Resolve<IUserService>(nameof(UserService));
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		#endregion

		#region Scoped

		[Fact]
		public async Task Can_Resolve_LifestyleScoped_From_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleScoped()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleScoped_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleScoped()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = container.Resolve<IUserService>(nameof(UserService));
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleScopedToNetServiceScope_From_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifeStyle.ScopedToNetServiceScope()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleScopedToNetServiceScope_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifeStyle.ScopedToNetServiceScope()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = container.Resolve<IUserService>(nameof(UserService));
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		#endregion

		#region Transient

		[Fact]
		public async Task Can_Resolve_LifestyleTransient_From_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleTransient()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleTransient_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleTransient()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = container.Resolve<IUserService>(nameof(UserService));
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleNetTransient_From_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleNetTransient()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		[Fact]
		public async Task Can_Resolve_LifestyleNetTransient_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleNetTransient()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			var actualUserService = sp.GetService<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state =>
			{
				try
				{
					var actualUserService = container.Resolve<IUserService>(nameof(UserService));
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					return;
				}
				tcs.SetResult(actualUserService);
			}, null);

			// Wait for the work item to complete.
			var task = tcs.Task;
			IUserService result = await task;
			Assert.NotNull(result);
		}

		#endregion
	}
}
