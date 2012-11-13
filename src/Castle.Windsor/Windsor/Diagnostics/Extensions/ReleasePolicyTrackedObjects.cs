// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
#if !SILVERLIGHT
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.Windsor.Diagnostics.DebuggerViews;
	using Castle.Windsor.Diagnostics.Helpers;

	public class ReleasePolicyTrackedObjects : AbstractContainerDebuggerExtension
	{
		private const string name = "Objects tracked by release policy";
		private TrackedComponentsDiagnostic diagnostic;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var result = diagnostic.Inspect();
			if (result == null)
			{
				return new DebuggerViewItem[0];
			}
			var item = BuildItem(result);
			if (item != null)
			{
				return new[] { item };
			}
			return new DebuggerViewItem[0];
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			diagnostic = new TrackedComponentsDiagnostic();
			diagnosticsHost.AddDiagnostic<ITrackedComponentsDiagnostic>(diagnostic);
		}

		private DebuggerViewItem BuildItem(ILookup<IHandler, object> results)
		{
			var totalCount = 0;
			var items = new List<DebuggerViewItem>();
			foreach (var result in results.OrderBy(l => l.Key.ComponentModel.Name))
			{
				var handler = result.Key;
				var objects = result.ToArray();
				totalCount += objects.Length;
				var view = ComponentDebuggerView.BuildFor(handler);
				var item = new DebuggerViewItem(handler.GetComponentName(),
				                                "Count = " + objects.Length,
				                                new MasterDetailsDebuggerViewItem(view, view.Description, "Component", objects));
				items.Add(item);
			}
			items.Sort((f, s) => f.Name.CompareTo(s.Name));
			return new DebuggerViewItem(name, "Count = " + totalCount, items.ToArray());
		}

		public static string Name
		{
			get { return name; }
		}
	}
#endif
}