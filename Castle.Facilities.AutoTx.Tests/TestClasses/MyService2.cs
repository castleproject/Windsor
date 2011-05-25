#region license

// // Copyright 2009-2011 Henrik Feldt - http://logibit.se /
// // 
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// // 
// //     http://www.apache.org/licenses/LICENSE-2.0
// // 
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Transactions;
using Castle.Facilities.Transactions;
using NUnit.Framework;

namespace Castle.Facilities.Transactions.Tests.TestClasses
{
	public class MyService2
	{
		private readonly ITransactionManager _Manager;

		public MyService2(ITransactionManager manager)
		{
			Contract.Ensures(_Manager != null);
			_Manager = manager;
		}

		[Transaction]
		public virtual ITransaction VerifyInAmbient()
		{
			Assert.That(System.Transactions.Transaction.Current != null,
			            "The current transaction mustn't be null.");

			Assert.That(_Manager.CurrentTransaction != null,
				"The current transaction in the transaction manager mustn't be null.");

			return _Manager.CurrentTransaction.Value;
		}

		[Transaction]
		public virtual void VerifyInAmbient(Action a)
		{
			Assert.That(System.Transactions.Transaction.Current != null,
			            "The current transaction mustn't be null in ambient case.");

			a();
		}

		public void VerifyInAmbient(Action a, Action before, Action after)
		{
			if (before != null) before();
			VerifyInAmbient(a);
			if (after != null) after();
		}

		public void VerifyBookKeepingInFork(Action a, Action before, Action after)
		{
			if (before != null) before();
			VerifyBookKeepingInFork(a);
			if (after != null) after();
		}

		[Transaction(Fork = true)]
		public virtual void VerifyBookKeepingInFork(Action action)
		{
			Assert.That(System.Transactions.Transaction.Current != null,
						"The current transaction mustn't be null in inner fork case.");

			action();
		}

		[Transaction(TransactionScopeOption.Suppress)]
		public virtual void VerifySupressed()
		{
			Assert.That(System.Transactions.Transaction.Current, Is.Null);
		}
	}
}