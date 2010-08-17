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

namespace Castle.Windsor.Debugging
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.MicroKernel;

	[DebuggerDisplay("")]
	internal class KernelDebuggerProxy
	{
		private readonly IEnumerable<IContainerDebuggerExtension> extensions;

		public KernelDebuggerProxy(IWindsorContainer container):this(container.Kernel){}

		public KernelDebuggerProxy(IKernel kernel)
		{
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			extensions = (IEnumerable<IContainerDebuggerExtension>)(kernel.GetSubSystem(SubSystemConstants.DebuggingKey) as IContainerDebuggerExtensionHost) ??
			             new IContainerDebuggerExtension[0];
		}

		[DebuggerDisplay("")]
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebuggerViewItemWithDescribtion[] Extensions
		{
			get { return extensions.SelectMany(e => e.Attach()).ToArray(); }
		}
	}
}