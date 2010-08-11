using System;
using log4net;

namespace Castle.Services.Transaction
{
	public sealed class TalkativeTransaction : TransactionBase, IEventPublisher
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (TalkativeTransaction));
		private bool _IsAmbient;

		public event EventHandler<TransactionEventArgs> TransactionCompleted;
		public event EventHandler<TransactionFailedEventArgs> TransactionFailed;
		public event EventHandler<TransactionEventArgs> TransactionRolledBack;

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

		public override void Begin()
		{
			try
			{
				base.Begin();
			}
			catch (TransactionException e)
			{
				_Logger.TryLogFail(() => TransactionFailed.Fire(this, new TransactionFailedEventArgs(this, e)));
				throw;
			}
		}

		protected override void InnerBegin() { }

		public override void Commit()
		{
			try
			{
				base.Commit();
				_Logger.TryLogFail(() => TransactionCompleted.Fire(this, new TransactionEventArgs(this)));
			}
			catch (TransactionException e)
			{
				_Logger.TryLogFail(() => TransactionFailed.Fire(this, new TransactionFailedEventArgs(this, e)));
				throw;
			}
		}

		protected override void InnerCommit() { }

		public override void Rollback()
		{
			try
			{
				base.Rollback();
				_Logger.TryLogFail(() => TransactionRolledBack.Fire(this, new TransactionEventArgs(this)));
			}
			catch (TransactionException e)
			{
				_Logger.TryLogFail(() => TransactionFailed.Fire(this, new TransactionFailedEventArgs(this, e)));
				throw;
			}
		}

		protected override void InnerRollback() {}
	}
}