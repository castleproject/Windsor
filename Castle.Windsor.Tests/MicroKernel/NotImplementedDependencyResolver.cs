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

namespace Castle.Windsor.Tests.MicroKernel
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;

	public class NotImplementedDependencyResolver : IDependencyResolver
	{
		public IKernel Kernel { get; set; }

		public void Initialize(IKernel kernel, DependencyDelegate resolving)
		{
			Kernel = kernel;
		}

		public void AddSubResolver(ISubDependencyResolver subResolver)
		{
			throw new NotImplementedException();
		}

		public void RemoveSubResolver(ISubDependencyResolver subResolver)
		{
			throw new NotImplementedException();
		}

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                       DependencyModel dependency)
		{
			throw new NotImplementedException();
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                      DependencyModel dependency)
		{
			throw new NotImplementedException();
		}
	}
}