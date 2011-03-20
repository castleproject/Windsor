// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Resolvers
{
#if !DOTNET35
	using System;
	using System.Collections;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel.Registration;



	/// <summary>
	///   Lazily adds component for <see cref = "Lazy{T}" />.
	/// </summary>
	[Singleton]
	public class LazyOfTComponentLoader : ILazyComponentLoader
	{
		private static readonly MethodInfo getRegistration = typeof(LazyOfTComponentLoader)
			.GetMethod("GetRegistration", BindingFlags.NonPublic | BindingFlags.Instance);

		private readonly IKernel kernel;

		public LazyOfTComponentLoader(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public IRegistration Load(string key, Type service, IDictionary arguments)
		{
			if (service == null)
			{
				return null;
			}
			if (service.IsGenericType == false)
			{
				return null;
			}
			if (service.GetGenericTypeDefinition() != typeof(Lazy<>))
			{
				return null;
			}
			return (IRegistration)getRegistration
			                      	.MakeGenericMethod(service.GetGenericArguments())
			                      	.Invoke(this, new object[] { arguments });
		}

		private IRegistration GetRegistration<TService>(IDictionary arguments)
		{
			return Component.For<Lazy<TService>>()
				.LifeStyle.Transient
				.NamedAutomatically(GetName(typeof(TService)))
				.DependsOn(Property.ForKey("value").Is(typeof(TService)),
				           Property.ForKey<Func<TService>>().Eq(new Func<TService>(kernel.Resolve<TService>)))
				.DependsOn(arguments)
				.OnDestroy((k, l) =>
				{
					if (l.IsValueCreated)
					{
						k.ReleaseComponent(l.Value);
					}
				});
		}

		private string GetName(Type service)
		{
			if (string.IsNullOrEmpty(service.FullName))
			{
				return "auto-lazy: " + Guid.NewGuid();
			}
			return "auto-lazy: " + service.FullName;
		}
	}
#endif
}