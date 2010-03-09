using System.Collections;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// Emulates a standalone transaction but in fact it 
	/// just propages a transaction. 
	/// </summary>
	public class ChildTransaction : StandardTransaction
	{
		private StandardTransaction _parent;

		public ChildTransaction(StandardTransaction parent) : 
			base(parent.TransactionMode, parent.IsolationMode, parent.IsAmbient)
		{
			_parent = parent;
		}

		public override void Enlist(IResource resource)
		{
			_parent.Enlist(resource);
		}

		public override void Begin()
		{
			// Ignored
		}

		public override void Rollback()
		{
			// Vote as rollback

			_parent.SetRollbackOnly();
		}

		public override void Commit()
		{
			// Vote as commit
		}

		public override void SetRollbackOnly()
		{
			Rollback();
		}

		public override void RegisterSynchronization(ISynchronization synchronization)
		{
			_parent.RegisterSynchronization(synchronization);
		}

		public override IDictionary Context
		{
			get { return _parent.Context; }
		}

		public override bool IsChildTransaction
		{
			get { return true; }
		}

		public override bool IsRollbackOnlySet
		{
			get { return _parent.IsRollbackOnlySet; }
		}
	}
}