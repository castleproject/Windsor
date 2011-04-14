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

namespace Castle.Windsor.Diagnostics.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.Windsor.Diagnostics.DebuggerViews;

#if !SILVERLIGHT
	public class DefaultComponentPerService : AbstractContainerDebuggerExtension
	{
		private const string name = "Default component per service";
		private IKernel kernel;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var map = new Dictionary<Type, IHandler>();
			BuildMap(map, kernel);
			if (map.Count == 0)
			{
				return Enumerable.Empty<DebuggerViewItem>();
			}
			var items = map.OrderBy(p => p.Key.Name).Select(p => DefaultComponentView(p.Value, p.Key.Name)).ToArray();
			return new[]
			{
				new DebuggerViewItem(name, "Count = " + items.Length, items)
			};
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			this.kernel = kernel;
		}

		private void BuildMap(Dictionary<Type, IHandler> map, IKernel currentKernel)
		{
			var defaults = currentKernel.GetSubSystem(SubSystemConstants.NamingKey) as IExposeDefaultComponentsForServices;
			if (defaults != null)
			{
				foreach (var @default in defaults.GetDefaultComponentsForServices())
				{
					if (map.ContainsKey(@default.Key))
					{
						continue;
					}
					map.Add(@default.Key, @default.Value);
				}
			}
			var parentKernel = currentKernel.Parent;
			if (parentKernel != null)
			{
				BuildMap(map, parentKernel);
			}
		}
	}
#endif
}