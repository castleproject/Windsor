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
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.Windsor.Experimental.Diagnostics.DebuggerViews;
	using Castle.Windsor.Experimental.Diagnostics.Helpers;

	public class PotentialLifestyleMismatches : AbstractContainerDebuggerExtension
	{
		private const string name = "Potential Lifestyle Mismatches";
		private IPotentialLifestyleMismatchesDiagnostic diagnostic;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var mismatches = diagnostic.Inspect();
			if (mismatches.Length == 0)
			{
				yield break;
			}

			var items = Array.ConvertAll(mismatches, MismatchedComponentView);
			yield return new DebuggerViewItem(name, "Count = " + mismatches.Length, items);
		}

		public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
		{
			diagnostic = new PotentialLifestyleMismatchesDiagnostic(kernel);
			diagnosticsHost.AddDiagnostic(diagnostic);
		}

		private MismatchedDependencyDebuggerViewItem GetItem(IHandler[] handlers)
		{
			var message = GetMismatchMessage(handlers);
			return new MismatchedDependencyDebuggerViewItem(message, handlers);
		}

		private string GetKey(IHandler root)
		{
			return string.Format("\"{0}\" »{1}«", GetNameDescription(root.ComponentModel), root.ComponentModel.GetLifestyleDescription());
		}

		private string GetMismatchMessage(IHandler[] handlers)
		{
			var message = new StringBuilder();
			Debug.Assert(handlers.Length > 1, "handlers.Length > 1");
			var root = handlers.First();
			var last = handlers.Last();
			message.AppendFormat("Component '{0}' with lifestyle {1} ", GetNameDescription(root.ComponentModel), root.ComponentModel.GetLifestyleDescription());
			message.AppendFormat("depends on '{0}' with lifestyle {1}", GetNameDescription(last.ComponentModel), last.ComponentModel.GetLifestyleDescription());

			for (var i = 1; i < handlers.Length - 1; i++)
			{
				var via = handlers[i];
				message.AppendLine();
				message.AppendFormat("\tvia '{0}' with lifestyle {1}", GetNameDescription(via.ComponentModel), via.ComponentModel.GetLifestyleDescription());
			}

			message.AppendLine();
			message.AppendFormat(
				"This kind of dependency is usually not desired and may lead to various kinds of bugs.");
			return message.ToString();
		}

		private string GetName(IHandler[] handlers, IHandler root)
		{
			var indirect = (handlers.Length > 2) ? "indirectly " : string.Empty;
			return string.Format("\"{0}\" »{1}« {2}depends on", GetNameDescription(root.ComponentModel), root.ComponentModel.GetLifestyleDescription(), indirect);
		}

		private string GetNameDescription(ComponentModel componentModel)
		{
			if (componentModel.ComponentName.SetByUser)
			{
				return componentModel.ComponentName.Name;
			}
			return componentModel.ToString();
		}

		private DebuggerViewItem MismatchedComponentView(IHandler[] handlers)
		{
			var item = GetItem(handlers);
			var key = GetKey(handlers.Last());
			var name = GetName(handlers, handlers.First());
			return new DebuggerViewItem(name, key, item);
		}

		public static string Name
		{
			get { return name; }
		}
	}
#endif
}