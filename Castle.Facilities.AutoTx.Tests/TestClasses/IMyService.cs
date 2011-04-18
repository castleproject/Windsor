using System;
using System.Transactions;

namespace Castle.Services.Transaction.Tests.vNext
{
	public interface IMyService
	{
		vNextTransaction.ITransaction VerifyInAmbient();
		void VerifyInAmbient(Action a);
		void VerifySupressed();
	}
}