﻿using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
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
	[CollectionDefinition(nameof(DoNotParallelize), DisableParallelization = true)]
	public class DoNotParallelize { }

	/// <summary>
	/// These is the original Castle Windsor Dependency Injection behavior.
	/// </summary>
	public class ResolveFromThreadpoolUnsafe_NetStatic : AbstractResolveFromThreadpoolUnsafe
	{
		public ResolveFromThreadpoolUnsafe_NetStatic() : base(false)
		{
		}

		#region "Singleton"

		/// <summary>
		/// This test will Succeed is we use standard Castle Windsor Singleton lifestyle instead of the custom
		/// NetStatic lifestyle.
		/// </summary>
		[Fact]
		public async Task Can_Resolve_LifestyleNetStatic_From_WindsorContainer_NoRootScopeAvailable()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			using var f = new WindsorServiceProviderFactory(container);
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
			var ex = await Catches.ExceptionAsync(async () =>
			{
				var task = tcs.Task;
				IUserService result = await task;
				//with the fix we can now use correctly a fallback for the root scope so we can access root scope even
				//if we are outside of scope
				Assert.NotNull(result);
			});

			// This test will fail if we use NetStatic lifestyle
			Assert.Null(ex);

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		#endregion
	}

	/// <summary>
	/// Mapping NetStatic to usual Singleton lifestyle.
	/// </summary>
	public class ResolveFromThreadpoolUnsafe_Singleton : AbstractResolveFromThreadpoolUnsafe
	{
		public ResolveFromThreadpoolUnsafe_Singleton() : base(true)
		{
		}

		#region "Singleton"

		/// <summary>
		/// This test will Succeed is we use standard Castle Windsor Singleton lifestyle instead of the custom
		/// NetStatic lifestyle.
		/// </summary>
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
			var ex = await Catches.ExceptionAsync(async () =>
			{
				var task = tcs.Task;
				IUserService result = await task;
				// The test succeeds if we use standard Castle Windsor Singleton lifestyle instead of the custom NetStatic lifestyle.
				Assert.NotNull(result);
			});

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		#endregion
	}

	/// <summary>
	/// relying on static state (WindsorDependencyInjectionOptions) is not good for tests
	/// that might run in parallel, can lead to false positives / negatives.
	/// </summary>
	[Collection(nameof(DoNotParallelize))]
	public abstract class AbstractResolveFromThreadpoolUnsafe
	{
		protected AbstractResolveFromThreadpoolUnsafe(bool mapNetStaticToSingleton)
		{
			WindsorDependencyInjectionOptions.MapNetStaticToSingleton = mapNetStaticToSingleton;
		}

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

		#endregion

		#region Scoped

		/*
		 * Scoped tests might fail if for whatever reason you do not have a current scope
		 * (like when you run from Threadpool.UnsafeQueueUserWorkItem).
		 */

		/// <summary>
		/// This test will fail because the service provider adapter
		/// does not create a standard Castle Windsor scope
		/// </summary>
		[Fact]
		public async Task Cannot_Resolve_LifestyleScoped_From_ServiceProvider()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			using var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleScoped()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			// must create a standard Castle Windsor scope (not managed by the adapter)
			using (var s = container.BeginScope())
			{
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
				var ex = await Catches.ExceptionAsync(async () =>
				{
					var task = tcs.Task;
					IUserService result = await task;
					Assert.NotNull(result);
				});

				Assert.NotNull(ex);
				Assert.IsType<InvalidOperationException>(ex);
				Assert.StartsWith("Scope was not available. Did you forget to call container.BeginScope()?", ex.Message);
			}

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		/// <summary>
		/// This test will fail because the service provider adapter
		/// does not create a standard Castle Windsor scope
		/// </summary>
		[Fact]
		public async Task Cannot_Resolve_LifestyleScoped_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			using var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				Component.For<IUserService>().ImplementedBy<UserService>().LifestyleScoped()
			);

			IServiceProvider sp = f.CreateServiceProvider(container);

			// must create a standard Castle Windsor scope (not managed by the adapter)
			using (var s = container.BeginScope())
			{
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
				var ex = await Catches.ExceptionAsync(async () =>
				{
					var task = tcs.Task;
					IUserService result = await task;
					Assert.NotNull(result);
				});

				Assert.NotNull(ex);
				Assert.IsType<InvalidOperationException>(ex);
				Assert.StartsWith("Scope was not available. Did you forget to call container.BeginScope()?", ex.Message);
			}

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		/// <summary>
		/// This test succeeds because WindsorScopedServiceProvider captured the root scope on creation
		/// and forced it to be current before service resolution.
		/// Scoped is tied to the rootscope = potential memory leak.
		/// </summary>
		[Fact]
		public async Task Can_Resolve_LifestyleScopedToNetServiceScope_From_ServiceProvider_MemoryLeak()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			using var f = new WindsorServiceProviderFactory(container);
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
		public async Task Cannot_Resolve_LifestyleScopedToNetServiceScope_From_WindsorContainer()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			using var f = new WindsorServiceProviderFactory(container);
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
			var ex = await Catches.ExceptionAsync(async () =>
			{
				var task = tcs.Task;
				IUserService result = await task;
				Assert.NotNull(result);
			});

			Assert.NotNull(ex);
			Assert.IsType<ComponentResolutionException>(ex);
			Assert.StartsWith("Could not obtain scope for component", ex.Message);

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
			using var f = new WindsorServiceProviderFactory(container);
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
			using var f = new WindsorServiceProviderFactory(container);
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
		public async Task Can_Resolve_LifestyleNetTransient_From_ServiceProvider_MemoryLeak()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			using var f = new WindsorServiceProviderFactory(container);
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
		public async Task Cannot_Resolve_LifestyleNetTransient_From_WindsorContainer_NoScopeAvailable()
		{
			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			using var f = new WindsorServiceProviderFactory(container);
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
			var ex = await Catches.ExceptionAsync(async () =>
			{
				var task = tcs.Task;
				IUserService result = await task;
				Assert.NotNull(result);
			});

			Assert.NotNull(ex);
			Assert.IsType<ComponentResolutionException>(ex);
			Assert.StartsWith("Could not obtain scope for component", ex.Message);

			(sp as IDisposable)?.Dispose();
			container.Dispose();
		}

		#endregion

		/*
		 * Missing tests: we should also test what happens with injected IServiceProvider (what scope do they get?)
		 * Injected IServiceProvider might or might not have a scope (it depends on AsyncLocal value).
		 */
	}

	public static class Catches
	{
		public static Exception Exception(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				return e;
			}
			return null;
		}

		public async static Task<Exception> ExceptionAsync(Func<Task> func)
		{
			try
			{
				await func();
			}
			catch (Exception e)
			{
				return e;
			}
			return null;
		}
	}
}
