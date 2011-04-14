using System;

namespace Castle.Services.Transaction.Tests.vNext
{
	public interface IMyService
	{
		[Transaction]
		vNextTransaction.ITransaction VerifyInAmbient();

		[Transaction]
		void VerifyInAmbient(Action a);
	}
}