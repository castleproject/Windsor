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
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.Windsor;

	public static class WindsorContainerExtensions
	{
		/// <summary>
		/// For grabbing a hold of the <see cref="AspNetCoreFacility"/> during middleware registration from the Configure(IApplicationBuilder, IHostingEnvironment, ILoggerFactory) method in Startup. 
		/// </summary>
		/// <typeparam name="T">The <see cref="IFacility"/> implementation</typeparam>
		/// <param name="container">A reference to <see cref="IWindsorContainer"/></param>
		/// <returns>An implementation of <see cref="IFacility"/></returns>
		public static T GetFacility<T>(this IWindsorContainer container) where T : IFacility
		{
			return (T) container.Kernel.GetFacilities().FirstOrDefault(x => x.GetType() == typeof(T));
		}
	}
}