using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using log4net;

namespace Castle.Services.vNextTransaction
{
	/// <summary>
	/// Value-object that encapsulates a transaction and is serializable across
	/// app-domains.
	/// </summary>
	[Serializable]
	public sealed class Activity : MarshalByRefObject, IEquatable<Activity>
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (Activity));

		private readonly Guid _ActivityId = Guid.NewGuid();
		private readonly Stack<ITransaction> _Txs = new Stack<ITransaction>();

		public Maybe<ITransaction> CurrentTransaction
		{
			get
			{
				Contract.Ensures(Contract.Result<Maybe<ITransaction>>() != null);
				return _Txs.Count == 0 ? Maybe.None<ITransaction>() : Maybe.Some(_Txs.Peek());
			}
		}


		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_Txs != null);
		}

		/// <summary>
		/// Push a transaction onto the stack of transactions.
		/// </summary>
		/// <param name="transaction"></param>
		public void Push(ITransaction transaction)
		{
			Contract.Requires(transaction != null);
			Contract.Ensures(Contract.OldValue(Count) + 1 == Count);
			
			// I can't prove this because Push doesn't have those contracts
			//Contract.Ensures(Contract.Exists(_Txs, x => object.ReferenceEquals(x, transaction)));
			
			// I can't prove this because I can't reason about value/reference equality using reflection in Maybe
			//Contract.Ensures(object.ReferenceEquals(CurrentTransaction.Value, transaction));
			
			_Logger.DebugFormat("pushing tx#{0}", transaction.TransactionInformation.LocalIdentifier);

			_Txs.Push(transaction);
		}

		/// <summary>
		/// Return the top-most transaction from the stack of transactions.
		/// </summary>
		/// <returns></returns>
		public ITransaction Pop()
		{
			Contract.Requires(Count > 0);
			// I can't prove this because Push doesn't have those contracts
			// Contract.Ensures(Contract.ForAll(_Txs, x => !object.ReferenceEquals(x, Contract.Result<ITransaction>())));
			Contract.Ensures(Contract.OldValue(Count) - 1 == Count);
			var ret = _Txs.Pop();

			_Logger.DebugFormat("popping tx#{0}", ret.TransactionInformation.LocalIdentifier);

			return ret;
		}

		public int Count { get { return _Txs.Count; } }

		[Pure]
		public bool Equals(Activity other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.GetHashCode().Equals(GetHashCode());
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Activity)) return false;
			return Equals((Activity) obj);
		}

		public override int GetHashCode()
		{
			return _ActivityId.GetHashCode();
		}

		public static bool operator ==(Activity left, Activity right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Activity left, Activity right)
		{
			return !Equals(left, right);
		}
	}
}