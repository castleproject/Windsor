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

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;

	using Castle.Windsor.Extensions.DependencyInjection.Tests.AspNetCoreApp;

	using Microsoft.AspNetCore.Mvc.Testing;

	using Xunit;

	public class AspNetCoreScopeTests
	{
		[Fact]
		public async Task TestScopeIsAvailable()
		{
			WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
			HttpClient client = factory.CreateClient();
			HttpResponseMessage response = await client.GetAsync("/");
			Assert.NotNull(response);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}
	}
}
