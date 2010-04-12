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

namespace Castle.MicroKernel.Proxy
{
	using Castle.MicroKernel.Context;

	/// <summary>
	/// Represents obtained just in time object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReference<T>
	{
		/// <summary>
		/// Resolves object referenced by this reference, optionally using provided <paramref name="kernel"/>.
		/// If object is resolved from the kernel, the <paramref name="context"/>should be used to guard
		/// against against cyclic dependencies.
		/// </summary>
		/// <param name="kernel"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		T Resolve(IKernel kernel, CreationContext context);
	}
}