// Copyright 2004-2023 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Extensions.DependencyInjection.Tests.AspNetCoreApp
{
	using Microsoft.AspNetCore.Builder;
#if NETCOREAPP3_1
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;
#endif
	using Microsoft.Extensions.Hosting;

	public class Program
	{
#if NET6_0
		public static void Main(string[] args)
		{
			// create web application builder
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
			
			// configure container
			WindsorContainer container = new WindsorContainer();
			WindsorServiceProviderFactory factory = new WindsorServiceProviderFactory(container);
			IHostBuilder _ = builder.Host.UseServiceProviderFactory(factory);
			
			// configure pipeline
			WebApplication app = builder.Build();
			app.MapGet("/", () => "Hello world");

			// start
			app.Run();
		}
#elif NETCOREAPP3_1
		public static void Main(string[] args)
		{
			// create host builder
			IHost app =
				CreateHostBuilder(args)
					.Build();
			
			// start
			app.Run();
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			WindsorContainer container = new WindsorContainer();
			WindsorServiceProviderFactory factory = new WindsorServiceProviderFactory(container);
			return Host
				.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(factory)
				.ConfigureWebHostDefaults(hostBuilder => hostBuilder.UseStartup<Startup>());
		}

		public class Startup
		{
			public void ConfigureServices(IServiceCollection serviceCollection)
			{
			}

			public void Configure(IApplicationBuilder app)
			{
				app.Map(
					string.Empty,
					appBuilder =>
					{
						appBuilder.Run(async (context) => await context.Response.WriteAsync("Hello world"));
					});
			}
		}
#endif
	}
}