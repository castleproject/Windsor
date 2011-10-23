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
	using Castle.Core.Internal;
	using Castle.MicroKernel;

	public class PotentiallyMisconfiguredComponentsDiagnostic : IPotentiallyMisconfiguredComponentsDiagnostic
	{
		private readonly IKernel kernel;

		public PotentiallyMisconfiguredComponentsDiagnostic(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public IHandler[] Inspect()
		{
			var allHandlers = kernel.GetAssignableHandlers(typeof(object));
			var waitingHandlers = allHandlers.FindAll(IsWaitingForDependencies);
			return waitingHandlers;
		}

		private bool IsWaitingForDependencies(IHandler handler)
		{
			return handler.CurrentState == HandlerState.WaitingDependency;
		}
	}
}