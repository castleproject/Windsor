namespace Castle.Services.Transaction
{
	/// <summary>
	/// Emulates a standalone transaction but in fact it 
	/// just propages a transaction. 
	/// </summary>
	public sealed class ChildTransaction : TransactionBase
	{
		private readonly ITransaction _Parent;

		public ChildTransaction(ITransaction parent) 
			: base(string.Format("Child-TX to \"{0}\"", parent.Name),
				   parent.TransactionMode, parent.IsolationMode)
		{
			_Parent = parent;
		}

		public override void Begin()
		{
		}

		protected override void InnerBegin()
		{
		}

		protected override void InnerCommit()
		{
		}

		public override void Rollback()
		{
			// Vote as rollback
			_Parent.SetRollbackOnly();
		}

		public override void SetRollbackOnly()
		{
			_Parent.SetRollbackOnly();
		}

		protected override void InnerRollback()
		{
		}

		public override void Commit()
		{
		}

		public override bool IsChildTransaction
		{
			get { return true; }
		}

		public override void Enlist(IResource resource)
		{
			_Parent.Enlist(resource);
		}

		public override bool IsAmbient { 
			get { return true; }
			protected set { } 
		}

		public override bool IsRollbackOnlySet
		{
			get { return _Parent.IsRollbackOnlySet; }
		}
	}
}