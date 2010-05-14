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

namespace Castle.MicroKernel.Registration.Proxy
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using Castle.MicroKernel.Proxy;

	public class MixinRegistration : IEnumerable<IReference<object>>
	{
		private readonly IList<IReference<object>> mixins = new List<IReference<object>>();

		public MixinRegistration Objects(params object[] mixIns)
		{
			foreach (var mixIn in mixIns)
			{
				mixins.Add(new InstanceReference<object>(mixIn));
			}
			return this;
		}

		public MixinRegistration Service<TService>()
		{
			mixins.Add(new ComponentReference(typeof(TService)));
			return this;
		}

		public MixinRegistration Service(Type serviceType)
		{
			mixins.Add(new ComponentReference(serviceType));
			return this;
		}

		public MixinRegistration Service(string name)
		{
			mixins.Add(new ComponentReference<object>(name));
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mixins.GetEnumerator();
		}

		IEnumerator<IReference<object>> IEnumerable<IReference<object>>.GetEnumerator()
		{
			return mixins.GetEnumerator();
		}
	}
}