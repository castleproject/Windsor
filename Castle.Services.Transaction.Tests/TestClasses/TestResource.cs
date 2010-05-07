using System;

namespace Castle.Services.Transaction.Tests
{
	internal class TestResource : ResourceImpl
	{
		private readonly Action _S;
		private readonly Action _C;
		private readonly Action _R;

		public TestResource(Action s, Action c, Action r)
		{
			_S = s;
			_C = c;
			_R = r;
		}

		public override void Start()
		{
			base.Start();
			_S();
		}

		public override void Commit()
		{
			base.Commit();
			_C();
		}

		public override void Rollback()
		{
			base.Rollback();
			_R();
		}
	}
}