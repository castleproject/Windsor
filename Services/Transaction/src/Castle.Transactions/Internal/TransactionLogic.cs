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

namespace Castle.Transactions.Internal
{
	using System;

	/// <summary>
	/// Class with implementation-detail type algorithms.
	/// </summary>
	public static class TransactionLogic
	{
		/// <summary>
		/// Gets whether the transaction options dictate that the transaction should
		/// be a fork of its parent given the stack depth it would be at, as denoted by
		/// the <see cref="nextStackDepth"/> parameter.
		/// </summary>
		/// <param name="transactionOptions">The options to check forking for.</param>
		/// <param name="nextStackDepth">The stack depth this fork would result in.</param>
		/// <returns>Whether the fork should continue.</returns>
		public static bool ShouldFork(this ITransactionOptions transactionOptions, uint nextStackDepth)
		{
			return transactionOptions.Fork && nextStackDepth > 1;
		}

		/// <summary>
		/// Returns a fork scope factory func.
		/// The func is a factory that creates an IDisposable. This IDisposable is a semaphore
		/// for the fork-cases, as it can be signalled from other threads. The IDisposable-part
		/// is for API convenience.
		/// </summary>
		/// <param name="manager">The transaction manager</param>
		/// <param name="tx">The transaction to use in the fork.</param>
		/// <returns>A factory function for the fork scope</returns>
		public static Func<IDisposable> ForkScopeFactory(this ITransactionManager manager, ITransaction tx)
		{
			return () =>
			{
				manager.Activities.GetCurrentActivity().Push(tx);
				return new TransactionManager.DisposableScope(manager.Activities.GetCurrentActivity().Pop);
			};
		}
	}
}