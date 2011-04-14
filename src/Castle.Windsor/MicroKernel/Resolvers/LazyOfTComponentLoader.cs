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

	using Castle.Core;
	using Castle.MicroKernel.Internal;
	using Castle.MicroKernel.Registration;

	/// <summary>
	///   Lazily adds component for <see cref = "Lazy{T}" />.
	/// </summary>
	[Singleton]
	public class LazyOfTComponentLoader : ILazyComponentLoader
	{
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
			return Component.For(typeof(Lazy<>))
				.ImplementedBy(typeof(LazyEx<>))
				.LifeStyle.Transient
				.NamedAutomatically("castle-auto-lazy");
		}
	}
#endif
}