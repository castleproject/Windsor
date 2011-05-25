#region license

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

#endregion

using System.Diagnostics.Contracts;
using Castle.Windsor;

namespace Castle.Facilities.Transactions.Testing
{
	/// <summary>
	/// 	Helper class for adding 'nifty' extensions to Windsor which ensures disposal/release of
	/// 	resources.
	/// </summary>
	public static class ResolveEx
	{
		/// <summary>
		/// 	Resolve the service denoted by T.
		/// </summary>
		/// <typeparam name = "T">The service to resolve.</typeparam>
		/// <param name = "container">The container to resolve from.</param>
		/// <returns>The IOResolveScope</returns>
		public static ResolveScope<T> ResolveScope<T>(this IWindsorContainer container)
			where T : class
		{
			Contract.Requires(container != null);
			return new ResolveScope<T>(container);
		}

		/// <summary>
		/// 	Resolve the service denoted by T. Beware that some of the components in the IO scope,
		/// 	namely the file and directory implementations are per-transaction and as such shouldn't be
		/// 	resolved unless there is an ambient transaction.
		/// </summary>
		/// <typeparam name = "T">The service to resolve.</typeparam>
		/// <param name = "container">The container to resolve from.</param>
		/// <returns>The IOResolveScope</returns>
		public static ResolveScope<T> ResolveIOScope<T>(this IWindsorContainer container)
			where T : class
		{
			Contract.Requires(container != null);
			return new IOResolveScope<T>(container);
		}
	}
}