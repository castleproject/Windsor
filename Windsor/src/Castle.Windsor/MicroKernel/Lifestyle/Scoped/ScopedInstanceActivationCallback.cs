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

	/// <summary>
	///   Delegate used by <see cref = "ILifetimeScope" /> to request a new instance to be created (that would be the first instance in that scope, subsequently reused).
	/// </summary>
	/// <param name = "afterCreated">Callback which should be invoken by provided delegate right after isntance gets created and before it burden gets tracked.
	///   The purpose if this callback is to include scope in decisions regarding tracking of the instance by <see
	///    cref = "IReleasePolicy" />.
	///   Depending on the scope implementation it may or may not provide its own end of lifetime detection mechanism.</param>
	/// <returns></returns>
	public delegate Burden ScopedInstanceActivationCallback(Action<Burden> afterCreated);
}