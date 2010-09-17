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

namespace Castle.Windsor.Experimental.Debugging.Primitives
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;

	public class LifestyleDependency : IComponentDebuggerExtension
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IHandler handler;

		private readonly KeyValuePair<string, IList<Type>> value;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly List<LifestyleDependency> items = new List<LifestyleDependency>();
		
		public LifestyleDependency(IHandler handler, KeyValuePair<string, IList<Type>> value)
		{
			this.handler = handler;
			this.value = value;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public int Count
		{
			get { return items.Count; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public bool Empty
		{
			get { return items.Count == 0; }
		}

		public void Add(LifestyleDependency dependency)
		{
			items.Add(dependency);
		}

		public bool Mismatched()
		{
			return MismatchedDirectly(handler) || MismatchedIndirectly();
		}

		private bool MismatchedDirectly(IHandler handler)
		{
			return handler.ComponentModel.LifestyleType == LifestyleType.Transient ||
			       handler.ComponentModel.LifestyleType == LifestyleType.PerWebRequest;
		}

		private bool MismatchedIndirectly()
		{
			return items.Any(i => i.Mismatched());
		}

		public IEnumerable<DebuggerViewItem> Attach()
		{
			return items.Select(
				item =>
				new DebuggerViewItem(
					string.Format("Mismatched {0} depedency on \"{1}\"",
					              (MismatchedDirectly(item.handler) ? "" : "indirect"), item.value.Key),
					item.handler.ComponentModel.LifestyleType.ToString(),
					new MismatchedDependency(new DefaultComponentView(item.handler, value.Value.ToArray()), "bla bla bla")));
		}
	}
}