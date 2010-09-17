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
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.Windsor.Experimental.Debugging.Primitives;

	public class PotentialLifestyleMismatches : AbstractContainerDebuggerExtension
	{
		private INamingSubSystem naming;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var all = GetKeyToHandlersLookup(naming.GetKey2Handler());
			var mismatches = new List<ComponentDebuggerView>();
			var component2Handlers = all.ToDictionary(p => p.Key.ComponentModel);
			foreach (var component in all)
			{
				mismatches.AddRange(GetMismatches(component, component2Handlers));
			}
			if (mismatches.Count == 0)
			{
				yield break;
			}
			yield return new DebuggerViewItem("Potential Lifestyle Mismatches",
			                                  "Count = " + mismatches.Count,
			                                  new ComponentDebuggerViewCollection(mismatches.ToArray()));
		}

		public override void Init(IKernel kernel)
		{
			naming = kernel.GetSubSystem(SubSystemConstants.NamingKey) as INamingSubSystem;
		}

		private LifestyleDependency GetMismatch(ComponentModel component,
		                                        IDictionary
		                                        	<ComponentModel, KeyValuePair<IHandler, KeyValuePair<string, IList<Type>>>>
		                                        	component2Handlers)
		{
			var pair = component2Handlers[component];
			var handler = pair.Key;
			var item = new LifestyleDependency(handler, pair.Value);
			if (item.Mismatched())
			{
				return item;
			}
			foreach (ComponentModel dependent in handler.ComponentModel.Dependents)
			{
				var mismatch = GetMismatch(dependent, component2Handlers);
				if (mismatch != null)
				{
					item.Add(mismatch);
				}
			}

			if (item.Mismatched())
			{
				return item;
			}
			return null;
		}

		private IEnumerable<ComponentDebuggerView> GetMismatches(
			KeyValuePair<IHandler, KeyValuePair<string, IList<Type>>> component,
			IDictionary<ComponentModel, KeyValuePair<IHandler, KeyValuePair<string, IList<Type>>>> component2Handlers)
		{
			var handler = component.Key;
			if (IsSingleton(handler) == false)
			{
				yield break;
			}
			var item = new LifestyleDependency(handler, component.Value);
			foreach (ComponentModel dependent in handler.ComponentModel.Dependents)
			{
				var mismatch = GetMismatch(dependent, component2Handlers);
				if (mismatch != null)
				{
					item.Add(mismatch);
				}
			}
			if (item.Empty == false)
			{
				yield return new ComponentDebuggerView(handler, component.Value, "Issues count = " + item.Count, item);
			}
		}

		private bool IsSingleton(IHandler component)
		{
			var lifestyle = component.ComponentModel.LifestyleType;
			return lifestyle == LifestyleType.Undefined || lifestyle == LifestyleType.Singleton;
		}
	}
}