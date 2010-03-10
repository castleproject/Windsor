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

using System;
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Tests
{
	/// <summary>
	/// Summary description for MockTransactionManager.
	/// </summary>
	public class MockTransactionManager : ITransactionManager
	{
		private int _committedCount;
		private MockTransaction _current;
		private int _rolledBackCount;
		private int _transactions;

		public int TransactionCount
		{
			get { return _transactions; }
		}

		public int CommittedCount
		{
			get { return _committedCount; }
		}

		public int RolledBackCount
		{
			get { return _rolledBackCount; }
		}

		#region ITransactionManager Members

		public event EventHandler<TransactionEventArgs> TransactionCreated;
		public event EventHandler<TransactionEventArgs> ChildTransactionCreated;
		public event EventHandler<TransactionEventArgs> TransactionDisposed;
		public event EventHandler<TransactionEventArgs> TransactionRolledBack;
		public event EventHandler<TransactionEventArgs> TransactionCompleted;
		public event EventHandler<TransactionFailedEventArgs> TransactionFailed;

		public ITransaction CreateTransaction(TransactionMode transactionMode, IsolationMode isolationMode,
		                                      bool isAmbient)
		{
			_current = new MockTransaction();

			_transactions++;

			return _current;
		}

		public ITransaction CreateTransaction(TransactionMode transactionMode, IsolationMode isolationMode)
		{
			return CreateTransaction(transactionMode, isolationMode, false);
		}

		public void Dispose(ITransaction tran)
		{
			var transaction = (MockTransaction) tran;

			if (transaction.Status == TransactionStatus.Committed)
			{
				_committedCount++;
			}
			else
			{
				_rolledBackCount++;
			}

			_current = null;
		}

		public ITransaction CurrentTransaction
		{
			get { return _current; }
		}

		#endregion
	}
}