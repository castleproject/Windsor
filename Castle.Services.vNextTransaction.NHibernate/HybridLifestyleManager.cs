#region license

// Copyright 2011 Castle Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Diagnostics.Contracts;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;

namespace Castle.Services.vNextTransaction.NHibernate
{
	/// <summary>
	/// Abstract hybrid lifestyle manager, with two underlying lifestyles
	/// </summary>
	/// <typeparam name="M1">Primary lifestyle manager which has its constructor resolved through
	/// the main kernel.</typeparam>
	/// <typeparam name="M2">Secondary lifestyle manager</typeparam>
	public abstract class HybridLifestyleManager<M1, M2> : AbstractLifestyleManager
		where M1 : class, ILifestyleManager
		where M2 : ILifestyleManager, new()
	{
		protected M1 lifestyle1;
		protected readonly M2 lifestyle2 = new M2();

		public override void Dispose()
		{
			if (lifestyle1 != null) lifestyle1.Dispose();
			lifestyle2.Dispose();
		}

		public override void Init(IComponentActivator componentActivator, IKernel kernel, ComponentModel model)
		{
			Contract.Ensures(lifestyle1 != null);

			using (var k = new DefaultKernel())
			{
				k.AddComponent("M1.lifestyle", typeof(M1), LifestyleType.Transient);
				kernel.AddChildKernel(k);

				try
				{
					lifestyle1 = k.Resolve<M1>();
				}
				finally
				{
					kernel.RemoveChildKernel(k);
				}
			}

			lifestyle1.Init(componentActivator, kernel, model);
			lifestyle2.Init(componentActivator, kernel, model);

			base.Init(componentActivator, kernel, model);
		}

		public override bool Release(object instance)
		{
			bool r1 = lifestyle1 != null ? lifestyle1.Release(instance) : false;
			bool r2 = lifestyle2.Release(instance);
			return r1 || r2;
		}

		public abstract override object Resolve(CreationContext context);
	}
}