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

namespace Castle.Facilities.AspNetCore
{
	using System;
	using System.Collections.Generic;

	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.DependencyInjection;

	internal static class AspNetCoreExtensions
	{
		public static void AddRequestScopingMiddleware(this IServiceCollection services, Func<IEnumerable<IDisposable>> requestScopeProvider)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			if (requestScopeProvider == null) throw new ArgumentNullException(nameof(requestScopeProvider));

			services.AddSingleton<IStartupFilter>(new RequestScopingStartupFilter(requestScopeProvider));
		}

		private sealed class RequestScopingStartupFilter : IStartupFilter
		{
			private readonly Func<IEnumerable<IDisposable>> requestScopeProvider;

			public RequestScopingStartupFilter(Func<IEnumerable<IDisposable>> requestScopeProvider)
			{
				this.requestScopeProvider = requestScopeProvider ?? throw new ArgumentNullException(nameof(requestScopeProvider));
			}

			public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> nextFilter)
			{
				return builder =>
				{
					ConfigureRequestScoping(builder);

					nextFilter(builder);
				};
			}

			private void ConfigureRequestScoping(IApplicationBuilder builder)
			{
				builder.Use(async (context, next) =>
				{
					var scopes = requestScopeProvider();
					try
					{
						await next();
					}
					finally
					{
						foreach (var scope in scopes)
						{
							scope.Dispose();
						}
					}
				});
			}
		}
	}
}