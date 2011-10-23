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

namespace Castle.Services.Transaction.Internal
{
	using System;
	using System.Transactions;

	using Castle.Core.Logging;

	/// <summary>
	/// 	A TxScope sets the ambient transaction for the duration of its lifetime and then re-assigns the previous value.
	/// 	This class is NOT for public consumption. Use it if you are dealing
	/// 	with <see cref = "CommittableTransaction" /> and <see cref = "DependentTransaction" />
	/// 	manually (and not using the transaction services to do it); otherwise
	/// 	the <see cref = "ITransactionManager" /> will take care of setting the 
	/// 	correct static properties for you.
	/// </summary>
	public sealed class TxScope : IDisposable
	{
		private readonly Transaction prev;
		private readonly ILogger _Logger;

		/// <summary>
		/// 	A TxScope sets the ambient transaction for the duration of its lifetime and then re-assigns the previous value.
		/// 	This class is NOT for public consumption. Use it if you are dealing
		/// 	with <see cref = "CommittableTransaction" /> and <see cref = "DependentTransaction" />
		/// 	manually (and not using the transaction services to do it); otherwise
		/// 	the <see cref = "ITransactionManager" /> will take care of setting the 
		/// 	correct static properties for you.
		/// </summary>
		public TxScope(Transaction curr, ILogger logger)
		{
			prev = Transaction.Current;
			Transaction.Current = curr;
			_Logger = logger;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool isManaged)
		{
			if (!isManaged)
			{
				if (_Logger.IsWarnEnabled) // does this always work?
					_Logger.Warn("TxScope Dispose wasn't called from managed context! You need to make sure that you dispose the scope, "
					             + "or you will break the Transaction.Current invariant of the framework and your own code by extension.");

				return;
			}
			Transaction.Current = prev;
		}
	}
}