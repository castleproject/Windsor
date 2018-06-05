﻿// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

	using Castle.Core.Internal;
	using Castle.MicroKernel;
	using Castle.Windsor.Diagnostics.DebuggerViews;

	public class AllServices : AbstractContainerDebuggerExtension
	{
		private const string name = "All services";
		private IAllServicesDiagnostic diagnostic;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var map = diagnostic.Inspect();
			var items = map.Select(p => BuildServiceView(p, p.Key.ToCSharpString())).ToArray();
			Array.Sort(items, (i1, i2) => i1.Name.CompareTo(i2.Name));
			return new[]
			{
				new DebuggerViewItem(name, "Count = " + items.Length, items)
			};
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			diagnostic = new AllServicesDiagnostic(kernel);
			diagnosticsHost.AddDiagnostic(diagnostic);
		}

		private DebuggerViewItem BuildServiceView(IEnumerable<IHandler> handlers, string name)
		{
			var components = handlers.Select(DefaultComponentView).ToArray();
			return new DebuggerViewItem(name, "Count = " + components.Length, components);
		}
	}
}