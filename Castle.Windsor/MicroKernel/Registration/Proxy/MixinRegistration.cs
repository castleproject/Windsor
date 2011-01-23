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

namespace Castle.MicroKernel.Registration.Proxy
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public class MixinRegistration : IEnumerable<IReference<object>>
	{
		private readonly IList<IReference<object>> items = new List<IReference<object>>();

		public MixinRegistration Objects(params object[] objects)
		{
			foreach (var item in objects)
			{
				items.Add(new InstanceReference<object>(item));
			}
			return this;
		}

		public MixinRegistration Service<TService>()
		{
			items.Add(new ComponentReference<object>(typeof(TService)));
			return this;
		}

		public MixinRegistration Service(Type serviceType)
		{
			items.Add(new ComponentReference<object>(serviceType));
			return this;
		}

		public MixinRegistration Service(string name)
		{
			items.Add(new ComponentReference<object>(name));
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator<IReference<object>> IEnumerable<IReference<object>>.GetEnumerator()
		{
			return items.GetEnumerator();
		}
	}
}