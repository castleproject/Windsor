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
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.Windsor.Diagnostics.DebuggerViews;

	public class DuplicatedDependenciesDebuggerExtension : AbstractContainerDebuggerExtension
	{
		private const string name = "Potentially duplicated dependencies";
		private DuplicatedDependenciesDiagnostic diagnostic;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var result = diagnostic.Inspect();
			if (result.Length == 0)
			{
				return Enumerable.Empty<DebuggerViewItem>();
			}
			var items = BuildItems(result);
			return new[]
			{
				new DebuggerViewItem(name, "Count = " + items.Length, items)
			};
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			diagnostic = new DuplicatedDependenciesDiagnostic(kernel);
			diagnosticsHost.AddDiagnostic<IDuplicatedDependenciesDiagnostic>(diagnostic);
		}

		private ComponentDebuggerView[] BuildItems(Pair<IHandler, Pair<DependencyModel, DependencyModel>[]>[] results)
		{
			return Array.ConvertAll(results, ComponentWithDuplicateDependenciesView);
		}

		private ComponentDebuggerView ComponentWithDuplicateDependenciesView(Pair<IHandler, Pair<DependencyModel, DependencyModel>[]> input)
		{
			var handler = input.First;
			var mismatches = input.Second;
			var items = Array.ConvertAll(mismatches, MismatchView);
			return ComponentDebuggerView.BuildRawFor(handler, "Count = " + mismatches.Length, items);
		}

		private DebuggerViewItem MismatchView(Pair<DependencyModel, DependencyModel> input)
		{
			return new DebuggerViewItem(Description(input.First), Description(input.Second), null);
		}

		public static string Name
		{
			get { return name; }
		}

		private static string Description(DependencyModel dependencyModel)
		{
			return dependencyModel.ToString();


		}
	}
#endif
}