using System;
using System.Collections;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// Emulates a standalone transaction but in fact it 
	/// just propages a transaction. 
	/// </summary>
	public sealed class ChildTransaction : TransactionBase
	{
		private readonly ITransaction _Parent;

		public ChildTransaction(string name, TransactionMode mode, IsolationMode isolationMode) : base(name, mode, isolationMode)
		{
		}

		public override void Begin()
		{
			// Ignored
		}

		internal override void InnerBegin()
		{
		}

		internal override void InnerCommit()
		{
		}

		public override void Rollback()
		{
			// Vote as rollback
			_Parent.SetRollbackOnly();
		}

		protected override void InnerRollback()
		{
		}

		public override void Commit()
		{
			// Vote as commit
		}

		public override bool IsChildTransaction
		{
			get { return true; }
		}

		public override bool IsAmbient { get; protected set; }
	}
}