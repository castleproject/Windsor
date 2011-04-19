using System;
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx.Tests.TestClasses
{
	public interface IMyService
	{
		ITransaction VerifyInAmbient();
		void VerifyInAmbient(Action a);
		void VerifySupressed();
	}
}