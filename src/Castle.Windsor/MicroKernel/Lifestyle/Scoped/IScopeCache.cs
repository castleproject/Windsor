// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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
	/// <summary>Represents persistence mechanism for instances of components that are supposed to be reused within scope managing the cache.</summary>
	public interface IScopeCache
	{
		/// <summary>
		///     Saves or retrieves a <see cref = "Burden" /> stored in the cache associated with the given <paramref name = "id" />.
		/// </summary>
		/// <exception cref = "T:System.ArgumentNullException">
		///     Thrown when <paramref name = "id" /> is null.
		/// </exception>
		/// <exception cref = "T:System.ArgumentException">
		///     Thrown when there is already a <see cref = "Burden" /> associated with given
		///     <paramref
		///         name = "id" />
		///     value in the cache.
		/// </exception>
		/// <remarks>The interface gives no thread safety guarantees. When the scope can be accessed from multiple threads the implementor should ensure thread safety.</remarks>
		Burden this[object id] { set; get; }
	}
}