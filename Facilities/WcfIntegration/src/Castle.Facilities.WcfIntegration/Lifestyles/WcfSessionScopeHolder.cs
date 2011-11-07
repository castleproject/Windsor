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

namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System;
	using System.ServiceModel;

	using Castle.MicroKernel.Lifestyle.Scoped;

	public class WcfSessionScopeHolder : IExtension<IContextChannel>, IDisposable
	{
		private readonly ILifetimeScope scope;

		public WcfSessionScopeHolder(DefaultLifetimeScope scope)
		{
			this.scope = scope;
		}

		public ILifetimeScope Scope
		{
			get { return scope; }
		}

		public void Dispose()
		{
			scope.Dispose();
		}

		private void Shutdown(object sender, EventArgs e)
		{
			Dispose();
		}

		void IExtension<IContextChannel>.Attach(IContextChannel owner)
		{
			owner.Faulted += Shutdown;
			owner.Closed += Shutdown;
		}

		void IExtension<IContextChannel>.Detach(IContextChannel owner)
		{
			owner.Faulted -= Shutdown;
			owner.Closed -= Shutdown;
		}
	}
}