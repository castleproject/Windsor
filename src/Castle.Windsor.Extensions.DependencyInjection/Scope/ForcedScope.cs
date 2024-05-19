// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Extensions.DependencyInjection.Scope
{
	using System;

	/// <summary>
	/// Forces a specific <see name="ExtensionContainerScope" /> for 'using' block. In .NET scope is tied to an instance of <see name="System.IServiceProvider" /> not a thread or async context
	/// </summary>
	internal class ForcedScope : IDisposable
	{
		private readonly ExtensionContainerScopeBase scope;
		private readonly ExtensionContainerScopeBase previousScope;
		internal ForcedScope(ExtensionContainerScopeBase scope, ExtensionContainerRootScope rootScope)
		{
			previousScope = ExtensionContainerScopeCache.TryCurrent ?? rootScope;
			this.scope = scope;
			ExtensionContainerScopeCache.Current = scope;
		}
		public void Dispose()
		{
			if(ExtensionContainerScopeCache.Current != scope) return;
			ExtensionContainerScopeCache.Current = previousScope;
		}
	}
}