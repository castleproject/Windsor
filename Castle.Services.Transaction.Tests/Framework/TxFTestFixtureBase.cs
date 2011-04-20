using System;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests.Framework
{
	public abstract class TxFTestFixtureBase
	{
		[TestFixtureSetUp]
		public void EnsureSupported()
		{
			if (!VerifySupported())
				return;
		}

		private static bool VerifySupported()
		{
			if (Environment.OSVersion.Version.Major < 6)
				Assert.Fail("OSVersion.Version.Major < 6 don't support transactional NTFS");

			return true;
		}
	}
}