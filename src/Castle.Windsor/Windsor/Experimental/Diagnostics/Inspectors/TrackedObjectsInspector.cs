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

namespace Castle.Windsor.Experimental.Diagnostics.Inspectors
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel;

	public class TrackedObjectsInspector : IDiagnosticsInspector<TrackedObjects, List<Burden>>, IDiagnosticsSource<IEnumerable<KeyValuePair<IHandler, object[]>>>
	{
		public void Inspect(TrackedObjects data, List<Burden> context)
		{
			if (data == null)
			{
				return;
			}
			context.AddRange(data.TrackedObjectBurdens);
			foreach (var subScope in data.SubScopes)
			{
				var diagnostics = subScope as IExposeDiagnostics<TrackedObjects>;
				if (diagnostics != null)
				{
					diagnostics.Visit(this, context);
				}
			}
		}

		public IEnumerable<KeyValuePair<IHandler, object[]>> Inspect(IKernel kernel)
		{
			var item = kernel.ReleasePolicy as IExposeDiagnostics<TrackedObjects>;
			if (item == null)
			{
				return null;
			}
			var context = new List<Burden>();
			item.Visit(this, context);
			return context.ToLookup(k => k.Handler)
				.ToDictionary(k => k.Key, v => v.Select(b => b.Instance).ToArray());
		}
	}
}