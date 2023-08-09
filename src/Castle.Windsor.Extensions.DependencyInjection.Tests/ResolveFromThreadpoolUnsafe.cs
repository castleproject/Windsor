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

		/* 
		 * Singleton tests should never fail, given you have a container instance you should always
		 * be able to resolve a singleton from it. 
		 */

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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
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
					var actualUserService = container.Resolve<IUserService>();
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
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
					var actualUserService = container.Resolve<IUserService>();
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		#endregion

		#region Scoped

		/*
		 * Scoped tests might fail if for whatever reason you do not have a current scope
		 * (like when you run from Threadpool.UnsafeQueueUserWorkItem).
		 */

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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
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
					var actualUserService = container.Resolve<IUserService>();
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		/// <summary>
		/// This test succeeds because WindsorScopedServiceProvider captured the root scope on creation
		/// and forced it to be current before service resolution.
		/// Scoped is tied to the rootscope = potential memory leak.
		/// </summary>
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
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
					var actualUserService = container.Resolve<IUserService>();
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		#endregion

		#region Transient

		/*
		 * Transient tests failure is questionable:
		 * - if you have a container you should be able to resolve transient without a scope,
		 *   but they might be tracked by the container itself (or the IServiceProvider)
		 * - when windsor container is disposed all transient services are disposed as well
		 * - when a IServiceProvider is disposed all transient services (created by it) are disposed as well
		 * - problem is: we have una instance of a windsor container passed on to multiple instances of IServiceProvider
		 *   one solution will be to tie the Transients to a scope, and the scope is tied to service provider
		 *   when both of them are disposed, the transient services are disposed as well
		 */

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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
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
					var actualUserService = container.Resolve<IUserService>();
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		/// <summary>
		/// This test succeeds because WindsorScopedServiceProvider captured the root scope on creation
		/// and forced it to be current before service resolution.
		/// Transient is tied to the rootscope = potential memory leak.
		/// </summary>
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
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
					var actualUserService = container.Resolve<IUserService>();
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

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		#endregion

		/*
		 * Missing tests: we should also test what happens with injected IServiceProvider (what scope do they get?)
		 * Injected IServiceProvider might or might not have a scope (it depends on AsyncLocal value).
		 */
	}
}
