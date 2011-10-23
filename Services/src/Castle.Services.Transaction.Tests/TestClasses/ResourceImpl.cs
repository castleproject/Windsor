#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
	using System;
	using System.Transactions;

	public class ResourceImpl : ISinglePhaseNotification
	{
		private bool _Prepared;
		private bool _RolledBack;
		private bool _Committed;
		private bool _WasDisposed;
		private bool _InDoubt;
		private bool _SinglePhaseCommitCalled;

		#region Props

		public bool Prepared
		{
			get { return _Prepared; }
		}

		public bool RolledBack
		{
			get { return _RolledBack; }
		}

		public bool Committed
		{
			get { return _Committed; }
		}

		public bool SinglePhaseCommitCalled
		{
			get { return _SinglePhaseCommitCalled; }
		}

		public bool WasDisposed
		{
			get { return _WasDisposed; }
		}

		#endregion

		public void Dispose()
		{
			_WasDisposed = true;
		}

		public virtual void Prepare(PreparingEnlistment preparingEnlistment)
		{
			if (_Prepared) throw new ApplicationException("prepare called before");
			_Prepared = true;
			preparingEnlistment.Prepared();
		}

		public virtual void Commit(Enlistment enlistment)
		{
			if (_Committed) throw new ApplicationException("commit called previously");
			_Committed = true;
			enlistment.Done();
		}

		public virtual void Rollback(Enlistment enlistment)
		{
			if (_RolledBack) throw new ApplicationException("Rollback called before");
			_RolledBack = true;
			enlistment.Done();
		}

		public virtual void InDoubt(Enlistment enlistment)
		{
			if (_InDoubt) throw new ApplicationException("in doubt method called twice");
			_InDoubt = true;
			enlistment.Done();
		}

		public virtual void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
		{
			if (_SinglePhaseCommitCalled) throw new ApplicationException("single phase commit called before");
			_SinglePhaseCommitCalled = true;
			singlePhaseEnlistment.Committed();
		}
	}
}