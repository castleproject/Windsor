#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace Castle.Facilities.Transactions.Tests
{
	using Castle.Facilities.Transactions.IO;
	using Castle.Facilities.Transactions.Tests.Framework;

	using NUnit.Framework;
	using Exts = Facilities.Transactions.Tests.TestClasses.Exts;

	public class DirectoryAdapter_ChJail : TxFTestFixtureBase
	{
		private string _CurrDir;

		[SetUp]
		public void SetUp()
		{
			_CurrDir =
				Path.GetPathWithoutLastBit(Path.GetFullPath(typeof(DirectoryAdapter_InitializationSettings).Assembly.CodeBase));
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
			Assert.IsTrue(d.IsInAllowedDir(Exts.Combine(_CurrDir, "hej/something/test")));
			Assert.IsTrue(d.IsInAllowedDir(Exts.Combine(_CurrDir, "hej")));
			Assert.IsTrue(d.IsInAllowedDir(Exts.Combine(_CurrDir, "hej.txt")));

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