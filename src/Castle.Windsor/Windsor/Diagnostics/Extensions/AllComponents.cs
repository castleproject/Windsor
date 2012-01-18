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

	using Castle.MicroKernel;
	using Castle.Windsor.Diagnostics.DebuggerViews;

#if !SILVERLIGHT
	public class AllComponents : AbstractContainerDebuggerExtension
	{
		private const string name = "All Components";

		private IAllComponentsDiagnostic diagnostic;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var handlers = diagnostic.Inspect();

			var items = Array.ConvertAll(handlers, DefaultComponentView);
			Array.Sort(items, (c1, c2) => c1.Name.CompareTo(c2.Name));
			return new[]
			{
				new DebuggerViewItem(name, "Count = " + items.Length, items)
			};
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			diagnostic = new AllComponentsDiagnostic(kernel);
			diagnosticsHost.AddDiagnostic(diagnostic);
		}

		public static string Name
		{
			get { return name; }
		}
	}
#endif
}