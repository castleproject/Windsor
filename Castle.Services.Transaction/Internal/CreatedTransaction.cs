using System;
using System.Diagnostics.Contracts;
using Castle.MicroKernel.Facilities;

namespace Castle.Services.Transaction.Internal
{
	internal sealed class CreatedTransaction : ICreatedTransaction
	{
		private readonly ITransaction _Transaction;
		private readonly bool _ShouldFork;
		private readonly Func<IDisposable> _ForkScopeFactory;

		public CreatedTransaction(ITransaction transaction, bool shouldFork, Func<IDisposable> forkScopeFactory)
		{
			Contract.Requires(forkScopeFactory != null);
			Contract.Requires(transaction != null);
			Contract.Requires(transaction.State == TransactionState.Active);

			_Transaction = transaction;
			_ShouldFork = shouldFork;
			_ForkScopeFactory = forkScopeFactory;
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Transaction != null);
			Contract.Invariant(_ForkScopeFactory != null);
		}

		#region Implementation of ICreatedTransaction

		ITransaction ICreatedTransaction.Transaction { get { return _Transaction; } }
		bool ICreatedTransaction.ShouldFork { get { return _ShouldFork; } }

		IDisposable ICreatedTransaction.GetForkScope()
		{
			var disposable = _ForkScopeFactory();

			if (disposable == null)
				throw new FacilityException("fork scope factory returned null!");

			return disposable;
		}

		#endregion
	}
}