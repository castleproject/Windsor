using System;
using log4net;

namespace Castle.Services.Transaction
{
	public sealed class TalkativeTransaction : TransactionBase, ITalkativeTransaction
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TalkativeTransaction));
		private bool _IsAmbient;
		

		public TalkativeTransaction(TransactionMode transactionMode, IsolationMode isolationMode, bool isAmbient) : 
			base(null, transactionMode, isolationMode)
		{
			_IsAmbient = isAmbient;
		}

		public override bool IsAmbient
		{
			get { return _IsAmbient; }
			protected set { _IsAmbient = value; }
		}

		internal override void InnerBegin()
		{
		}

		internal override void InnerCommit()
		{
		}

		public override void Rollback()
		{
			try
			{
				base.Rollback();
			}
			catch (TransactionException e)
			{
				_Logger.TryAndLog(() => TransactionFailed.Fire(this, new TransactionFailedEventArgs(this, e)));
				throw;
			}
			
		}

		public override void Commit()
		{
			try {
				base.Commit();
				_Logger.TryAndLog(() => TransactionCompleted.Fire(this, new TransactionEventArgs(this)));
			} 
			catch (TransactionException e)
			{
				_Logger.TryAndLog(() => TransactionFailed.Fire(this, new TransactionFailedEventArgs(this, e)));
				throw;
			}
		}

		protected override void InnerRollback()
		{
		}

		public event EventHandler<TransactionEventArgs> TransactionCompleted;
		public event EventHandler<TransactionFailedEventArgs> TransactionFailed;
	}
}