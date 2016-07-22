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

namespace Castle.MicroKernel.ModelBuilder.Descriptors
{
	using System;

	using Castle.Core;

	public class InterceptorDescriptor : IComponentModelDescriptor
	{
		private readonly int insertIndex;
		private readonly InterceptorReference[] interceptors;
		private readonly Where where;

		public InterceptorDescriptor(InterceptorReference[] interceptors, Where where)
		{
			this.interceptors = interceptors;
			this.where = where;
		}

		public InterceptorDescriptor(InterceptorReference[] interceptors, int insertIndex)
			: this(interceptors, Where.Insert)
		{
			if (insertIndex < 0)
			{
				throw new ArgumentOutOfRangeException("insertIndex", "insertIndex must be >= 0");
			}

			this.insertIndex = insertIndex;
		}

		public InterceptorDescriptor(InterceptorReference[] interceptors)
		{
			where = Where.Default;
			this.interceptors = interceptors;
		}

		public void BuildComponentModel(IKernel kernel, ComponentModel model)
		{
		}

		public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
		{
			foreach (var interceptor in interceptors)
			{
				switch (where)
				{
					case Where.First:
						model.Interceptors.AddFirst(interceptor);
						break;

					case Where.Last:
						model.Interceptors.AddLast(interceptor);
						break;

					case Where.Insert:
						model.Interceptors.Insert(insertIndex, interceptor);
						break;

					default:
						model.Interceptors.Add(interceptor);
						break;
				}
			}
		}

		public enum Where
		{
			First,
			Last,
			Insert,
			Default
		}
	}
}