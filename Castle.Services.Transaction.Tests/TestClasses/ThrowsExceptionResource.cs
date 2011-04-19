#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Transactions;

namespace Castle.Services.Transaction.Tests
{
	public class ThrowsExceptionResourceImpl : ResourceImpl
	{
		private readonly bool _ThrowOnCommit;
		private readonly bool _ThrowOnRollback;

		public ThrowsExceptionResourceImpl(bool throwOnCommit, bool throwOnRollback)
		{
			_ThrowOnCommit = throwOnCommit;
			_ThrowOnRollback = throwOnRollback;
		}

		public override void Rollback(Enlistment enlistment)
		{
			if (_ThrowOnRollback)
				throw new Exception("Simulated rollback error");

			base.Rollback(enlistment);
		}

		public override void Commit(Enlistment enlistment)
		{
			if (_ThrowOnCommit)
			{
				throw new Exception("Simulated commit error");
			}

			base.Commit(enlistment);
		}
	}
}