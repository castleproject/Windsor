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

namespace Castle.Windsor.Experimental.Debugging.Extensions
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.Windsor.Experimental.Debugging.Primitives;

	using ComponentsMap = System.Collections.Generic.IDictionary<Core.ComponentModel, Primitives.MetaComponent>;

	public class PotentialLifestyleMismatches : AbstractContainerDebuggerExtension
	{
		private INamingSubSystem naming;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var all = GetMetaComponents(naming.GetKey2Handler());
			var mismatches = new List<DebuggerViewItem>();
			var model2Meta = all.ToDictionary(p => p.Model);
			foreach (var component in model2Meta.Values)
			{
				mismatches.AddRange(GetMismatches(component, model2Meta));
			}
			if (mismatches.Count == 0)
			{
				yield break;
			}
			yield return new DebuggerViewItem("Potential Lifestyle Mismatches",
			                                  "Count = " + mismatches.Count,
			                                  mismatches.ToArray());
		}

		public override void Init(IKernel kernel)
		{
			naming = kernel.GetSubSystem(SubSystemConstants.NamingKey) as INamingSubSystem;
		}

		private IEnumerable<LifestyleDependency> GetMismatch(LifestyleDependency parent, ComponentModel component,
		                                                     ComponentsMap model2Meta)
		{
			var pair = model2Meta[component];
			var handler = pair.Handler;
			var item = new LifestyleDependency(pair, parent);
			if (item.Mismatched())
			{
				yield return item;
			}
			else
			{
				foreach (ComponentModel dependent in handler.ComponentModel.Dependents)
				{
					foreach (var mismatch in GetMismatch(item, dependent, model2Meta))
					{
						yield return mismatch;
					}
				}
			}
		}

		private IEnumerable<DebuggerViewItem> GetMismatches(MetaComponent component, ComponentsMap component2Handlers)
		{
			var handler = component.Handler;
			if (IsSingleton(handler) == false)
			{
				yield break;
			}
			var root = new LifestyleDependency(component);
			foreach (ComponentModel dependent in handler.ComponentModel.Dependents)
			{
				foreach (var mismatch in GetMismatch(root, dependent, component2Handlers))
				{
					yield return mismatch.MismatchView;
				}
			}
		}

		private bool IsSingleton(IHandler component)
		{
			var lifestyle = component.ComponentModel.LifestyleType;
			return lifestyle == LifestyleType.Undefined || lifestyle == LifestyleType.Singleton;
		}
	}
}