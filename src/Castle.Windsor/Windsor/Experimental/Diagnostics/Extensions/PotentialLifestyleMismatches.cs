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

namespace Castle.Windsor.Experimental.Diagnostics.Extensions
{
#if !SILVERLIGHT
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.Windsor.Experimental.Diagnostics.DebuggerViews;
	using Castle.Windsor.Experimental.Diagnostics.Helpers;

	public class PotentialLifestyleMismatches : AbstractContainerDebuggerExtension
	{
		private const string name = "Potential Lifestyle Mismatches";
		private INamingSubSystem naming;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var model2Handler = naming.GetAllHandlers().ToDictionary(p => p.ComponentModel);

			var mismatches = new List<DebuggerViewItem>();
			foreach (var handler in model2Handler.Values)
			{
				mismatches.AddRange(GetMismatches(model2Handler, handler));
			}
			if (mismatches.Count == 0)
			{
				yield break;
			}
			yield return new DebuggerViewItem(name,
			                                  "Count = " + mismatches.Count,
			                                  mismatches.ToArray());
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			naming = kernel.GetSubSystem(SubSystemConstants.NamingKey) as INamingSubSystem;
		}

		private IEnumerable<MismatchedLifestyleDependencyViewBuilder> GetMismatch(MismatchedLifestyleDependencyViewBuilder parent, ComponentModel component,
		                                                     Dictionary<ComponentModel, IHandler> model2Handler)
		{
			if (parent.Checked(component))
			{
				yield break;
			}

			var handler = model2Handler[component];
			var item = new MismatchedLifestyleDependencyViewBuilder(handler, parent);
			if (item.Mismatched())
			{
				yield return item;
			}
			else
			{
				foreach (ComponentModel dependent in handler.ComponentModel.Dependents)
				{
					foreach (var mismatch in GetMismatch(item, dependent, model2Handler))
					{
						yield return mismatch;
					}
				}
			}
		}

		private IEnumerable<DebuggerViewItem> GetMismatches(Dictionary<ComponentModel, IHandler> model2Handler, IHandler handler)
		{
			if (IsSingleton(handler) == false)
			{
				yield break;
			}
			var root = new MismatchedLifestyleDependencyViewBuilder(handler);
			foreach (ComponentModel dependent in handler.ComponentModel.Dependents)
			{
				foreach (var mismatch in GetMismatch(root, dependent, model2Handler))
				{
					yield return mismatch.ViewItem;
				}
			}
		}

		private bool IsSingleton(IHandler component)
		{
			var lifestyle = component.ComponentModel.LifestyleType;
			return lifestyle == LifestyleType.Undefined || lifestyle == LifestyleType.Singleton;
		}

		public static string Name
		{
			get { return name; }
		}
	}
#endif
}