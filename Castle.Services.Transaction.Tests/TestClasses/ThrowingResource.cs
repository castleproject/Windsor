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
	internal class ThrowingResource : ISinglePhaseNotification
	{
		private readonly bool _ThrowIt;
		private int _ErrorCount;

		public ThrowingResource(bool throwIt)
		{
			_ThrowIt = throwIt;
		}

		public bool WasRolledBack { get; private set; }

		#region Implementation of IEnlistmentNotification

		void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
		{
			preparingEnlistment.Prepared();
		}

		void IEnlistmentNotification.Commit(Enlistment enlistment)
		{
			if (_ThrowIt && ++_ErrorCount < 2)
				throw new ApplicationException("simulating resource failure");

			enlistment.Done();
		}

		void IEnlistmentNotification.Rollback(Enlistment enlistment)
		{
			WasRolledBack = true;

			enlistment.Done();
		}

		void IEnlistmentNotification.InDoubt(Enlistment enlistment)
		{
			enlistment.Done();
		}

		void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
		{
		}

		#endregion
	}
}