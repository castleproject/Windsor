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


namespace Castle.Facilities.AspNetCore.Tests.Fakes
{
	using System;
	using System.Threading.Tasks;

	using Microsoft.AspNetCore.Http;

	using Castle.MicroKernel.Lifestyle;

	public class AnyComponent { }
	public class AnyComponentWithLifestyleManager : AbstractLifestyleManager
	{
		public override void Dispose()
		{
		}
	}

	public sealed class AnyMiddleware : IMiddleware
	{
		private readonly AnyComponent anyComponent;
		private readonly ServiceProviderOnlyScopedDisposable serviceProviderOnlyScopedDisposable;
		private readonly WindsorOnlyScopedDisposable windsorOnlyScopedDisposable;
		private readonly CrossWiredScopedDisposable crossWiredScopedDisposable;

		public AnyMiddleware(
			ServiceProviderOnlyScopedDisposable serviceProviderOnlyScopedDisposable,
			WindsorOnlyScopedDisposable windsorOnlyScopedDisposable,
			CrossWiredScopedDisposable crossWiredScopedDisposable)
		{
			this.serviceProviderOnlyScopedDisposable = serviceProviderOnlyScopedDisposable ?? throw new ArgumentNullException(nameof(serviceProviderOnlyScopedDisposable));
			this.windsorOnlyScopedDisposable = windsorOnlyScopedDisposable ?? throw new ArgumentNullException(nameof(windsorOnlyScopedDisposable));
			this.crossWiredScopedDisposable = crossWiredScopedDisposable ?? throw new ArgumentNullException(nameof(crossWiredScopedDisposable));
		}

		public AnyMiddleware(AnyComponent anyComponent)
		{
			// This will never get called because Windsor picks the most greedy constructor
			this.anyComponent = anyComponent ?? throw new ArgumentNullException(nameof(anyComponent));
		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			// Do something before
			await next(context);
			// Do something after
		}
	}

}
