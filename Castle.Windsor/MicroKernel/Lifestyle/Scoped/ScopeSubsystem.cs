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

namespace Castle.MicroKernel.Lifestyle.Scoped
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public class ScopeSubsystem : AbstractSubSystem, IScopeManager
	{
		private readonly IScopeAccessor scopes;

		public ScopeSubsystem()
			: this(new ThreadScopeAccessor())
		{
		}

		public ScopeSubsystem(IScopeAccessor scopes)
		{
			if (scopes == null)
			{
				throw new ArgumentNullException("scopes");
			}

			this.scopes = scopes;
		}

		public LifestyleScope CurrentScope
		{
			get
			{
				var scopes = GetCurrentScopes();
				return (scopes.Count > 0) ? scopes.Peek() : null;
			}
		}

		public override void Init(IKernel kernel)
		{
			base.Init(kernel);

			kernel.Register(Component.For<IScopeManager>().Instance(this));
		}

		public IDisposable BeginScope()
		{
			var scope = new LifestyleScope();
			GetCurrentScopes().Push(scope);
			return new EndScope(this, scope);
		}

		public IDisposable RequireScope()
		{
			return (CurrentScope == null) ? BeginScope() : null;
		}

		private void EndCurrentScope(LifestyleScope scope)
		{
			var scopes = GetCurrentScopes();

			if (scopes.Peek() != scope)
			{
				throw new InvalidOperationException(
					"The scope is not current.  Did you forget to end a child scope?");
			}

			scope.Dispose();
			scopes.Pop();
		}

		private Stack<LifestyleScope> GetCurrentScopes()
		{
			var scopes = this.scopes.CurrentStack;
			if (scopes == null)
			{
				throw new InvalidOperationException("Unable to determine current scopes.  " +
				                                    "Did you provide the correct IScopeAccessor strategy?");
			}
			return scopes;
		}

		private class EndScope : IDisposable
		{
			private readonly ScopeSubsystem _manager;
			private readonly LifestyleScope _scope;

			public EndScope(ScopeSubsystem manager, LifestyleScope scope)
			{
				_manager = manager;
				_scope = scope;
			}

			public void Dispose()
			{
				_manager.EndCurrentScope(_scope);
			}
		}
	}
}