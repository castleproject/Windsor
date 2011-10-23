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

namespace Castle.Transactions
{
	using System.Diagnostics.Contracts;
	using System.Threading.Tasks;

	using Castle.Transactions.Contracts;

	/// <summary>
	/// 	An interface specifying whether the <see cref = "ITransaction" /> implementation
	/// 	knows about its dependents. If the transaction class does not implement this interface
	/// 	then dependent transcations that fail will not be awaited on the main thread, but instead
	/// 	on the finalizer thread (not good!).
	/// </summary>
	[ContractClass(typeof(IDependentAwareContract))]
	public interface IDependentAware
	{
		/// <summary>
		/// 	Registers a dependent task to wait for after Complete or Rollback has been called.
		/// </summary>
		/// <param name = "task">The task to await.</param>
		void RegisterDependent(Task task);
	}
}