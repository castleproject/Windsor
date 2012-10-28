// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System;
	using System.Linq;

	using Castle.MicroKernel;

	/// <summary>
	/// 	Collects all handlers for components in hosting container grouped by services they expose.
	/// 	Within the service group, first one would be the default (the one obtained when callling <see cref = "IKernel.Resolve(System.Type)" /> for the service type)
	/// </summary>
	public interface IAllServicesDiagnostic : IDiagnostic<ILookup<Type, IHandler>>
	{
	}
}