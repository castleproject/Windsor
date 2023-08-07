using Castle.MicroKernel.Registration;
using Castle.Windsor.Extensions.DependencyInjection.Extensions;
using Castle.Windsor.Extensions.DependencyInjection.Tests.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests {
	public class ResolveFromThreadpoolUnsafe {
		[Fact]
		public async Task Can_Resolve_From_ServiceProvider() {

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

			/*
			ThreadPool.UnsafeQueueUserWorkItem(state => {
				// resolving using castle (without scopes) works
				var actualUserService = container.Resolve<IUserService>(nameof(UserService));
				Assert.NotNull(actualUserService);
			}, null);
			*/

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state => {
				try {
					var actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex) {
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
		public async Task Can_Resolve_From_CastleWindsor() {

			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				// Component.For<IUserService>().ImplementedBy<UserService>().LifestyleNetTransient(),
				Classes.FromThisAssembly().BasedOn<IUserService>().WithServiceAllInterfaces().LifestyleNetStatic()
				);

			IUserService actualUserService;
			actualUserService = container.Resolve<IUserService>();
			Assert.NotNull(actualUserService);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state => {
				IUserService actualUserService = null;
				try {
					// resolving (with the underlying Castle Windsor, not using Service Provider) with a lifecycle that has an
					// accessor that uses something that is AsyncLocal might be troublesome.
					// the custom lifecycle accessor will kicks in, but noone assigns the Current scope (which is uninitialized)
					actualUserService = container.Resolve<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex) {
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
		public async Task Can_Resolve_From_ServiceProvider_cretaed_in_UnsafeQueueUserWorkItem() {

			var serviceProvider = new ServiceCollection();
			var container = new WindsorContainer();
			var f = new WindsorServiceProviderFactory(container);
			f.CreateBuilder(serviceProvider);

			container.Register(
				// Component.For<IUserService>().ImplementedBy<UserService>().LifestyleNetTransient(),
				Classes.FromThisAssembly().BasedOn<IUserService>().WithServiceAllInterfaces().LifestyleNetStatic()
				);

			TaskCompletionSource<IUserService> tcs = new TaskCompletionSource<IUserService>();

			ThreadPool.UnsafeQueueUserWorkItem(state => {
				IUserService actualUserService = null;
				try {
					// creating a service provider here will be troublesome too
					IServiceProvider sp = f.CreateServiceProvider(container);

					actualUserService = sp.GetService<IUserService>();
					Assert.NotNull(actualUserService);
				}
				catch (Exception ex) {
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
	}
}
