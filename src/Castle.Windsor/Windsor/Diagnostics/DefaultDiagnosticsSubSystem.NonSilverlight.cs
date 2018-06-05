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

namespace Castle.Windsor.Diagnostics
{
	using System.Collections;
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.Windsor.Diagnostics.Extensions;

	public partial class DefaultDiagnosticsSubSystem : IDiagnosticsHost, IContainerDebuggerExtensionHost
	{
		private readonly IList<IContainerDebuggerExtension> extensions = new List<IContainerDebuggerExtension>();

		public override void Init(IKernelInternal kernel)
		{
			base.Init(kernel);
			InitStandardExtensions();
		}

		public void Add(IContainerDebuggerExtension item)
		{
			item.Init(Kernel, this);
			extensions.Add(item);
		}

		public IEnumerator<IContainerDebuggerExtension> GetEnumerator()
		{
			return extensions.GetEnumerator();
		}

		protected virtual void InitStandardExtensions()
		{
			Add(new AllComponents());
			Add(new AllServices());
			Add(new PotentiallyMisconfiguredComponents());
			Add(new PotentialLifestyleMismatches());
			Add(new DuplicatedDependenciesDebuggerExtension());
			Add(new UsingContainerAsServiceLocator());
			Add(new ReleasePolicyTrackedObjects());
			Add(new Facilities());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}