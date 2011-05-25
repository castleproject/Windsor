namespace Castle.Facilities.Transactions.Tests.TestClasses
{
	using System;
	using System.Transactions;

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