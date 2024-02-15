#if NET8_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	public abstract class CustomAssumptionTests : IDisposable
	{
		private IServiceProvider _serviceProvider;

		[Fact]
		public void Resolve_All()
		{
			var serviceCollection = GetServiceCollection();
			serviceCollection.AddKeyedSingleton<ITestService, TestService>("one");
			serviceCollection.AddKeyedSingleton<ITestService, AnotherTestService>("one");
			serviceCollection.AddTransient<ITestService, AnotherTestService>();
			_serviceProvider = BuildServiceProvider(serviceCollection);

			// resolve all non-keyed services
			var services = _serviceProvider.GetServices<ITestService>();
			Assert.Single(services);
			Assert.IsType<AnotherTestService>(services.First());

			// passing "null" as the key should return all non-keyed services
			var keyedServices = _serviceProvider.GetKeyedServices<ITestService>(null);
			Assert.Single(keyedServices);
			Assert.IsType<AnotherTestService>(keyedServices.First());

			// resolve all keyed services
			keyedServices = _serviceProvider.GetKeyedServices<ITestService>("one");
			Assert.Equal(2, keyedServices.Count());
			Assert.IsType<TestService>(keyedServices.First());
			Assert.IsType<AnotherTestService>(keyedServices.Last());
		}

		[Fact]
		public void Scoped_keyed_service_resolved_by_thread_outside_scope()
		{
			Boolean stop = false;
			Boolean shouldResolve = false;
			ITestService resolvedInThread = null;
			var thread = new Thread(_ =>
			{
				while (!stop)
				{
					Thread.Sleep(100);
					if (shouldResolve)
					{
						stop = true;
						resolvedInThread = _serviceProvider.GetRequiredKeyedService<ITestService>("porcodio");
					}
				}
			});
			thread.Start();

			var serviceCollection = GetServiceCollection();
			serviceCollection.AddKeyedScoped<ITestService, TestService>("porcodio");
			_serviceProvider = BuildServiceProvider(serviceCollection);

			//resolved outside scope
			ITestService resolvedOutsideScope = _serviceProvider.GetRequiredKeyedService<ITestService>("porcodio");

			// resolve in scope
			ITestService resolvedInScope;
			using (var scope = _serviceProvider.CreateScope())
			{
				resolvedInScope = scope.ServiceProvider.GetRequiredKeyedService<ITestService>("porcodio");
			}

			shouldResolve = true;
			//now wait for the original thread to finish
			thread.Join(1000 * 10);
			Assert.NotNull(resolvedInThread);
			Assert.NotNull(resolvedOutsideScope);
			Assert.NotNull(resolvedInScope);

			Assert.NotEqual(resolvedInScope, resolvedOutsideScope);
			Assert.NotEqual(resolvedInScope, resolvedInThread);
			Assert.Equal(resolvedOutsideScope, resolvedInThread);
		}

		[Fact]
		public void Scoped_service_resolved_outside_scope()
		{
			var serviceCollection = GetServiceCollection();
			serviceCollection.AddScoped<ITestService, TestService>();
			_serviceProvider = BuildServiceProvider(serviceCollection);

			//resolved outside scope
			ITestService resolvedOutsideScope = _serviceProvider.GetRequiredService<ITestService>();
			Assert.NotNull(resolvedOutsideScope);

			// resolve in scope
			ITestService resolvedInScope;
			using (var scope = _serviceProvider.CreateScope())
			{
				resolvedInScope = scope.ServiceProvider.GetRequiredService<ITestService>();
			}
			Assert.NotNull(resolvedInScope);
			Assert.NotEqual(resolvedInScope, resolvedOutsideScope);

			ITestService resolvedAgainOutsideScope = _serviceProvider.GetRequiredService<ITestService>();
			Assert.NotNull(resolvedAgainOutsideScope);
			Assert.Equal(resolvedOutsideScope, resolvedAgainOutsideScope);
		}

		[Fact]
		public void Scoped_service_resolved_outside_scope_in_another_thread()
		{
			var serviceCollection = GetServiceCollection();
			serviceCollection.AddScoped<ITestService, TestService>();
			_serviceProvider = BuildServiceProvider(serviceCollection);

			var task = Task.Run(() =>
			{
				//resolved outside scope
				ITestService resolvedOutsideScope = _serviceProvider.GetRequiredService<ITestService>();
				Assert.NotNull(resolvedOutsideScope);

				// resolve in scope
				ITestService resolvedInScope;
				using (var scope = _serviceProvider.CreateScope())
				{
					resolvedInScope = scope.ServiceProvider.GetRequiredService<ITestService>();
				}
				Assert.NotNull(resolvedInScope);
				Assert.NotEqual(resolvedInScope, resolvedOutsideScope);

				ITestService resolvedAgainOutsideScope = _serviceProvider.GetRequiredService<ITestService>();
				Assert.NotNull(resolvedAgainOutsideScope);
				Assert.Equal(resolvedOutsideScope, resolvedAgainOutsideScope);
				return true;
			});

			Assert.True(task.Result);
		}

		[Fact]
		public async void Scoped_service_resolved_outside_scope_in_another_unsafe_thread()
		{
			var serviceCollection = GetServiceCollection();
			serviceCollection.AddScoped<ITestService, TestService>();
			_serviceProvider = BuildServiceProvider(serviceCollection);

			var tsc = new TaskCompletionSource();
			var worker = new QueueUserWorkItemWorker(_serviceProvider, tsc);
			ThreadPool.UnsafeQueueUserWorkItem(worker, false);
			await tsc.Task;

			Assert.Null(worker.ExecuteException);
			Assert.NotNull(worker.ResolvedOutsideScope);
			Assert.NotNull(worker.ResolvedInScope);
			Assert.NotEqual(worker.ResolvedInScope, worker.ResolvedOutsideScope);
		}

		[Fact]
		public async void Simulate_async_timer_without_wait()
		{
			Boolean stop = false;
			Boolean shouldResolve = false;
			ITestService resolvedInThread = null;
			async Task ExecuteAsync()
			{
				while (!stop)
				{
					await Task.Delay(100);
					if (shouldResolve)
					{
						stop = true;
						resolvedInThread = _serviceProvider.GetService<ITestService>();
					}
				}
			}
			//fire and forget
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			var task = ExecuteAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			await Task.Delay(500);

			var serviceCollection = GetServiceCollection();
			serviceCollection.AddScoped<ITestService, TestService>();
			_serviceProvider = BuildServiceProvider(serviceCollection);

			//resolved outside scope
			ITestService resolvedOutsideScope = _serviceProvider.GetRequiredService<ITestService>();

			// resolve in scope
			ITestService resolvedInScope;
			using (var scope = _serviceProvider.CreateScope())
			{
				resolvedInScope = scope.ServiceProvider.GetRequiredService<ITestService>();
			}

			shouldResolve = true;
			await task;
			Assert.NotNull(resolvedInThread);
			Assert.NotNull(resolvedOutsideScope);
			Assert.NotNull(resolvedInScope);

			Assert.NotEqual(resolvedInScope, resolvedOutsideScope);
			Assert.NotEqual(resolvedInScope, resolvedInThread);
			Assert.Equal(resolvedOutsideScope, resolvedInThread);
		}

		private class QueueUserWorkItemWorker : IThreadPoolWorkItem
		{
			private readonly IServiceProvider _provider;
			private readonly TaskCompletionSource _taskCompletionSource;

			public QueueUserWorkItemWorker(IServiceProvider provider, TaskCompletionSource taskCompletionSource)
			{
				_provider = provider;
				_taskCompletionSource = taskCompletionSource;
			}

			public ITestService ResolvedOutsideScope { get; private set; }
			public ITestService ResolvedInScope { get; private set; }
			public Exception ExecuteException { get; private set; }

			public void Execute()
			{
				try
				{
					ResolvedOutsideScope = _provider.GetService<ITestService>();
					using (var scope = _provider.CreateScope())
					{
						ResolvedInScope = scope.ServiceProvider.GetRequiredService<ITestService>();
					}
				}
				catch (Exception ex)
				{
					ExecuteException = ex;
				}

				_taskCompletionSource.SetResult();
			}
		}

		protected abstract IServiceCollection GetServiceCollection();

		protected abstract IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Dispose managed resources
				if (_serviceProvider is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			// Dispose unmanaged resources
		}
	}

	public class RealCustomAssumptionTests : CustomAssumptionTests
	{
		protected override IServiceCollection GetServiceCollection()
		{
			return new RealTestServiceCollection();
		}

		protected override IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
		{
			return serviceCollection.BuildServiceProvider();
		}
	}

	public class CastleWindsorCustomAssumptionTests : CustomAssumptionTests
	{
		private WindsorServiceProviderFactory _factory;
		private IWindsorContainer _container;

		protected override IServiceCollection GetServiceCollection()
		{
			return new TestServiceCollection();
		}

		protected override IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
		{
			_factory = new WindsorServiceProviderFactory();
			_container = _factory.CreateBuilder(serviceCollection);
			return _factory.CreateServiceProvider(_container);
		}

		[Fact]
		public void Try_to_resolve_scoped_directly_with_castle_windsor_container()
		{
			var serviceCollection = GetServiceCollection();
			serviceCollection.AddScoped<ITestService, TestService>();
			var provider = BuildServiceProvider(serviceCollection);

			//resolved outside scope
			ITestService resolvedOutsideScope = _container.Resolve<ITestService>();
			Assert.NotNull(resolvedOutsideScope);

			// resolve in scope
			ITestService resolvedInScope;
			using (var scope = provider.CreateScope())
			{
				resolvedInScope = _container.Resolve<ITestService>();
			}
			Assert.NotNull(resolvedInScope);
			Assert.NotEqual(resolvedInScope, resolvedOutsideScope);

			ITestService resolvedAgainOutsideScope = _container.Resolve<ITestService>();
			Assert.NotNull(resolvedAgainOutsideScope);
			Assert.Equal(resolvedOutsideScope, resolvedAgainOutsideScope);
		}

		[Fact]
		public void TryToResolveScopedInOtherThread()
		{
			var serviceCollection = GetServiceCollection();
			serviceCollection.AddScoped<ITestService, TestService>();
			var provider = BuildServiceProvider(serviceCollection);

			var task = Task.Run(() =>
			{
				//resolved outside scope
				ITestService resolvedOutsideScope = _container.Resolve<ITestService>();
				Assert.NotNull(resolvedOutsideScope);

				// resolve in scope
				ITestService resolvedInScope;
				using (var scope = provider.CreateScope())
				{
					resolvedInScope = _container.Resolve<ITestService>();
				}
				Assert.NotNull(resolvedInScope);
				Assert.NotEqual(resolvedInScope, resolvedOutsideScope);

				ITestService resolvedAgainOutsideScope = _container.Resolve<ITestService>();
				Assert.NotNull(resolvedAgainOutsideScope);
				Assert.Equal(resolvedOutsideScope, resolvedAgainOutsideScope);
				return true;
			});

			Assert.True(task.Result);
		}


		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_factory.Dispose();
			}
		}
	}

	internal class TestService : ITestService
	{
	}

	internal class AnotherTestService : ITestService
	{
	}

	internal interface ITestService
	{
	}
}
#endif