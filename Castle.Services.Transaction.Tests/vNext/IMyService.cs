using System;

namespace Castle.Services.Transaction.Tests.vNext
{
	public interface IMyService
	{
		[Transaction]
		void VerifyInAmbient();

		[Transaction]
		void VerifyInAmbient(Action a);
	}
}