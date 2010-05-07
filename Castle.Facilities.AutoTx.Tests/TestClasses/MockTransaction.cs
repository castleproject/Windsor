using System;
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Tests
{
	public class MockTransaction : TransactionBase
	{
		public MockTransaction() : base(null, TransactionMode.Unspecified, IsolationMode.Unspecified)
		{
		}

		protected override void InnerBegin()
		{
		}

		protected override void InnerCommit()
		{
		}

		protected override void InnerRollback()
		{
		}

		public override bool IsChildTransaction
		{
			get { return false; }
		}

		public override bool IsAmbient
		{
			get { throw new NotImplementedException(); }
			protected set { throw new NotImplementedException(); }
		}
	}
}