using System;
using System.Diagnostics.Contracts;
using System.Transactions;
using Castle.Services.Transaction.Monads;

namespace Castle.Services.Transaction
{
	[ContractClassFor(typeof (ITxManager))]
	internal abstract class ITxManagerContract : ITxManager
	{
		public Maybe<ITransaction> CurrentTopTransaction
		{
			get
			{
				Contract.Ensures(Contract.Result<Maybe<ITransaction>>() != null);
				throw new NotImplementedException();
			}
		}

		
		public Maybe<ITransaction> CurrentTransaction
		{
			get
			{
				Contract.Ensures(Contract.Result<Maybe<ITransaction>>() != null);
				throw new NotImplementedException();
			}
		}

		public uint Count
		{
			get { throw new NotImplementedException(); }
		}

		public void AddRetryPolicy(string policyKey, Func<Exception, bool> retryPolicy)
		{
			Contract.Requires(policyKey != null);
			Contract.Requires(retryPolicy != null);
		}

		public void AddRetryPolicy(string policyKey, IRetryPolicy retryPolicy)
		{
			Contract.Requires(policyKey != null);
			Contract.Requires(retryPolicy != null);
		}

		public Maybe<ICreatedTransaction> CreateTransaction(ITransactionOptions transactionOptions)
		{
			Contract.Requires(transactionOptions != null);

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>() != null
			                 && (!Contract.Result<Maybe<ICreatedTransaction>>().HasValue
			                     || Contract.Result<Maybe<ICreatedTransaction>>().Value.Transaction.State == TransactionState.Active));

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>().HasValue 
			                 || transactionOptions.Mode == TransactionScopeOption.Suppress);

			throw new NotImplementedException();
		}

		public Maybe<ICreatedTransaction> CreateFileTransaction(ITransactionOptions transactionOptions)
		{
			Contract.Requires(transactionOptions != null);

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>() != null
							 && (!Contract.Result<Maybe<ICreatedTransaction>>().HasValue
								 || Contract.Result<Maybe<ICreatedTransaction>>().Value.Transaction.State == TransactionState.Active));

			Contract.Ensures(Contract.Result<Maybe<ICreatedTransaction>>().HasValue
							 || transactionOptions.Mode == TransactionScopeOption.Suppress);

			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}
	}
}