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
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Releasers;
	using Castle.Windsor.Experimental.Diagnostics.Primitives;

#if !SILVERLIGHT
	public class LifecycledTrackedObjects : AbstractContainerDebuggerExtension
	{
		private const string name = "Objects tracked by release policy";
		private IKernel kernel;

		public override IEnumerable<DebuggerViewItem> Attach()
		{
			var policy = kernel.ReleasePolicy;
			if (policy == null)
			{
				return new DebuggerViewItem[0];
			}
			if (policy is LifecycledComponentsReleasePolicy)
			{
				var localPolicy = policy as LifecycledComponentsReleasePolicy;
				var items = localPolicy.TrackedObjects;
				return new[] { new DebuggerViewItem(name, "Count = " + items.Length, items) };
			}
			if (policy is NoTrackingReleasePolicy)
			{
				return new[] { new DebuggerViewItem(name, "No objects are ever tracked", null) };
			}
			return new[] { new DebuggerViewItem(name, "Not supported with " + policy.GetType().Name, null) };
		}

		public override void Init(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public static string Name
		{
			get { return name; }
		}
	}
#endif
}