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

using System;
using System.Transactions;

namespace Castle.Facilities.Transactions.Internal
{
	using Core.Logging;

	/// <summary>
	/// A TxScope sets the ambient transaction for the duration of its lifetime and then re-assigns the previous value.
	/// This class is NOT for public consumption. Use it if you are dealing
	/// with <see cref="CommittableTransaction"/> and <see cref="DependentTransaction"/>
	/// manually (and not using the transaction services to do it); otherwise
	/// the <see cref="ITransactionManager"/> will take care of setting the 
	/// correct static properties for you.
	/// </summary>
	public sealed class TxScope : IDisposable
	{
		private ILogger _Logger = NullLogger.Instance;

		private readonly System.Transactions.Transaction prev;

		/// <summary>
		/// A TxScope sets the ambient transaction for the duration of its lifetime and then re-assigns the previous value.
		/// This class is NOT for public consumption. Use it if you are dealing
		/// with <see cref="CommittableTransaction"/> and <see cref="DependentTransaction"/>
		/// manually (and not using the transaction services to do it); otherwise
		/// the <see cref="ITransactionManager"/> will take care of setting the 
		/// correct static properties for you.
		/// </summary>
		public TxScope(System.Transactions.Transaction curr)
		{
			prev = System.Transactions.Transaction.Current;
			System.Transactions.Transaction.Current = curr;
		}

		public ILogger Logger
		{
			get { return _Logger; }
			set { _Logger = value; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isManaged)
		{
			if (!isManaged)
			{
				_Logger.Warn("TxScope Dispose wasn't called from managed context! You need to make sure that you dispose the scope, "
				             + "or you will break the Transaction.Current invariant of the framework and your own code by extension.");

				return;
			}
			System.Transactions.Transaction.Current = prev;
		}
	}
}