// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Registration.Lifestyle
{
	using System;
	using Castle.Core;

	public class LifestyleGroup<S> : RegistrationGroup<S>
	{
		public LifestyleGroup(ComponentRegistration<S> registration)
			: base(registration)
		{
		}

		/// <summary>
		/// Sets the lifestyle to the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public ComponentRegistration<S> Is(LifestyleType type)
		{
			if (Enum.IsDefined(typeof(LifestyleType), type) == false)
			{
				throw InvalidValue(type, "Not a valid lifestyle");
			}
			if(type == LifestyleType.Undefined)
			{
				throw InvalidValue(type,string.Format("{0} is not a valid lifestyle type.", LifestyleType.Undefined));
			}

			return AddDescriptor(new LifestyleDescriptor<S>(type));
		}

		private ArgumentOutOfRangeException InvalidValue(LifestyleType type, string message)
		{
#if SILVERLIGHT
			return new ArgumentOutOfRangeException("type", message);
#else
			return new ArgumentOutOfRangeException("type", type, message);
#endif
		}

		public ComponentRegistration<S> Transient
		{
			get { return AddDescriptor(new LifestyleDescriptor<S>(LifestyleType.Transient)); }
		}

		public ComponentRegistration<S> Singleton
		{
			get { return AddDescriptor(new LifestyleDescriptor<S>(LifestyleType.Singleton)); }
		}

		public ComponentRegistration<S> PerThread
		{
			get { return AddDescriptor(new LifestyleDescriptor<S>(LifestyleType.Thread)); }
		}
		
#if (!SILVERLIGHT)
		public ComponentRegistration<S> PerWebRequest
		{
			get { return AddDescriptor(new LifestyleDescriptor<S>(LifestyleType.PerWebRequest)); }
		}
#endif

		public ComponentRegistration<S> Pooled
		{
			get { return AddDescriptor(new LifestyleDescriptor<S>(LifestyleType.Pooled)); }
		}

		public ComponentRegistration<S> PooledWithSize(int? initialSize, int? maxSize)
		{
			var pooledWithSize = Pooled;
			if(initialSize.HasValue)
			{
				pooledWithSize = pooledWithSize.Attribute("initialPoolSize").Eq(initialSize);
			}
			if(maxSize.HasValue)
			{
				pooledWithSize = pooledWithSize.Attribute("maxPoolSize").Eq(maxSize);
			}
			return pooledWithSize;
		}

		/// <summary>
		/// Assign a custom lifestyle type, that implements <see cref="ILifestyleManager"/>.
		/// </summary>
		/// <param name="customLifestyleType">Type of the custom lifestyle.</param>
		/// <returns></returns>
		public ComponentRegistration<S> Custom(Type customLifestyleType)
		{
			if (!typeof(ILifestyleManager).IsAssignableFrom(customLifestyleType))
			{
				throw new ComponentRegistrationException(String.Format(
					"The type {0} must implement ILifestyleManager to " +
					"be used as a custom lifestyle", customLifestyleType.FullName));
			}

			return AddDescriptor(new LifestyleDescriptor<S>(LifestyleType.Custom))
				.Attribute("customLifestyleType").Eq(customLifestyleType.AssemblyQualifiedName);
		}

		/// <summary>
		/// Assign a custom lifestyle type, that implements <see cref="ILifestyleManager"/>.
		/// </summary>
		/// <typeparam name="L">The type of the custom lifestyle</typeparam>
		/// <returns></returns>
		public ComponentRegistration<S> Custom<L>()
			where L : ILifestyleManager, new()
		{
			return Custom(typeof(L));
		}
	}
}
