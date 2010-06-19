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
	using System.Diagnostics;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Naming;

	internal class KernelDebuggerProxy
	{
		private IKernel kernel;
		private INamingSubSystem naming;

		public KernelDebuggerProxy(IKernel kernel)
		{
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			this.kernel = kernel;
			naming = kernel.GetSubSystem(SubSystemConstants.NamingKey) as INamingSubSystem;
		}

		[DebuggerDisplay("Count: {TotalHandlersCount}", Name = "AllComponents")]
		public HandlersByKeyDictionaryDebuggerView AllComponents
		{
			get { return new HandlersByKeyDictionaryDebuggerView(naming.GetKey2Handler()); }
		}

		[DebuggerDisplay("Count: {WaitingDependencyHandlersCount}", Name = "PotentiallyMisconfiguredComponents")]
		public HandlersByKeyDictionaryDebuggerView MisconfiguredHandlers
		{
			get
			{
				return
					new HandlersByKeyDictionaryDebuggerView(
						naming.GetKey2Handler().Where(h => h.Value.CurrentState == HandlerState.WaitingDependency));
			}
		}

		private int TotalHandlersCount
		{
			get { return naming.ComponentCount; }
		}

		private int WaitingDependencyHandlersCount
		{
			get { return MisconfiguredHandlers.Count; }
		}
	}
}