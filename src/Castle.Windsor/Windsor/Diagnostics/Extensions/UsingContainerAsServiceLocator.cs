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
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.Windsor.Diagnostics.DebuggerViews;

#if !SILVERLIGHT
	public class UsingContainerAsServiceLocator : AbstractContainerDebuggerExtension
	{
		private const string name = "Potential Service Locator usages";

		private IUsingContainerAsServiceLocatorDiagnostic diagnostic;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var handlers = diagnostic.Inspect();
			if (handlers.Length == 0)
			{
				return Enumerable.Empty<DebuggerViewItem>();
			}
			Array.Sort(handlers, (f, s) => f.ComponentModel.Name.CompareTo(s.ComponentModel.Name));
            var items = handlers.Select(h => DefaultComponentView(h)).ToArray();
			return new[]
			{
				new DebuggerViewItem(name, "Count = " + items.Length, items)
			};
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			diagnostic = new UsingContainerAsServiceLocatorDiagnostic(kernel);
			diagnosticsHost.AddDiagnostic(diagnostic);
		}
	}
#endif
}