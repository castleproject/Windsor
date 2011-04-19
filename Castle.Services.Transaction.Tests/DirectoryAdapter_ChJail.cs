using Castle.Services.Transaction.IO;
using NUnit.Framework;

namespace Castle.Services.Transaction.Tests
{
	public class DirectoryAdapter_ChJail
	{
		private string _CurrDir;

		[SetUp]
		public void SetUp()
		{
			_CurrDir = Path.GetPathWithoutLastBit(Path.GetFullPath(typeof(DirectoryAdapterTests).Assembly.CodeBase));
		}

		[Test]
		public void IsInAllowedDir_ReturnsFalseIfConstaint_AndOutside()
		{
			var d = new DirectoryAdapter(new MapPathImpl(), true, _CurrDir);

			Assert.IsFalse(d.IsInAllowedDir("\\"));
			Assert.IsFalse(d.IsInAllowedDir("\\\\?\\C:\\"));
			Assert.IsFalse(d.IsInAllowedDir(@"\\.\dev0"));
			Assert.IsFalse(d.IsInAllowedDir(@"\\?\UNC\/"));
		}

		[Test]
		public void IsInAllowedDir_ReturnsTrueForInside()
		{
			var d = new DirectoryAdapter(new MapPathImpl(), true, _CurrDir);
			Assert.IsTrue(d.IsInAllowedDir(_CurrDir));
			Assert.IsTrue(d.IsInAllowedDir(_CurrDir.Combine("hej/something/test")));
			Assert.IsTrue(d.IsInAllowedDir(_CurrDir.Combine("hej")));
			Assert.IsTrue(d.IsInAllowedDir(_CurrDir.Combine("hej.txt")));

			Assert.IsTrue(d.IsInAllowedDir("hej"), "It should return true for relative paths.");
			Assert.IsTrue(d.IsInAllowedDir("hej.txt"), "It should return true for relative paths");
		}

		[Test]
		public void IsInAllowedDirReturnsTrueIfNoConstraint()
		{
			var ad = new DirectoryAdapter(new MapPathImpl(), false, null);
			Assert.IsTrue(ad.IsInAllowedDir("\\"));
		}
	}
}