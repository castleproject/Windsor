#region license

// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace Castle.Services.Transaction.Tests
{
	using System.Transactions;

	using NUnit.Framework;

	public class TransactionManager_EnlistedResources
	{
		private ITxManager _Tm;

		[SetUp]
		public void SetUp()
		{
			_Tm = new TxManager(new TransientActivityManager());
		}

		[Test]
		[Ignore("Wait for RC")]
		public void TransactionResources_ForFileTransaction_AreDisposed()
		{
			var resource = new ResourceImpl();

			using (var tx = _Tm.CreateFileTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
				tx.Rollback();
			}

			Assert.That(resource.RolledBack);
		}

		[Test]
		public void TransactionResources_AreDisposed()
		{
			var resource = new ResourceImpl();

			using (var tx = _Tm.CreateTransaction(new DefaultTransactionOptions()).Value.Transaction)
			{
				tx.Inner.EnlistVolatile(resource, EnlistmentOptions.EnlistDuringPrepareRequired);
				tx.Rollback();
			}

			Assert.That(resource.RolledBack);
		}
	}
}