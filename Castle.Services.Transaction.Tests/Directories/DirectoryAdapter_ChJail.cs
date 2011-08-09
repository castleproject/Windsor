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

using System;
using Castle.IO;
using Castle.IO.Extensions;
using Castle.IO.Internal;
using Castle.Services.Transaction.Tests.Framework;
using Castle.Services.Transaction.Tests.TestClasses;
using Castle.Transactions.IO;
using NUnit.Framework;
using SharpTestsEx;

namespace Castle.Services.Transaction.Tests.Directories
{
	public class DirectoryAdapter_ChJail : TxFTestFixtureBase
	{
		private string _TfPath;

		[SetUp]
		public void TFSetup()
		{
			var dllPath = Environment.CurrentDirectory;
			_TfPath = dllPath.CombineAssert("DirectoryAdapter_ChJail");
		}

		[TearDown]
		public void TFTearDown()
		{
			Directory.Delete(_TfPath, true);
		}

		[Test]
		public void Smoke()
		{
		}

		[Test]
		public void IsInAllowedDir_ReturnsFalseIfConstaint_AndOutside()
		{
			var d = GetDirectoryAdapter();

			Assert.IsFalse(d.IsInAllowedDir("\\"));
			Assert.IsFalse(d.IsInAllowedDir("\\\\?\\C:\\"));
			Assert.IsFalse(d.IsInAllowedDir(@"\\.\dev0"));
			Assert.IsFalse(d.IsInAllowedDir(@"\\?\UNC\/"));
		}

		[Test]
		public void IsInAllowedDir_False_ForRelativePaths()
		{
			var d = GetDirectoryAdapter();

			d.IsInAllowedDir("../")
				.Should("because it's the parent dir")
				.Be.False();
		}

		[Test]
		public void True_ForAbsolute()
		{
			var d = GetDirectoryAdapter();

			d.IsInAllowedDir(_TfPath).Should().Be.True();
			d.IsInAllowedDir(_TfPath.Combine("A/B/C")).Should().Be.True();
			d.IsInAllowedDir(_TfPath.Combine("hej")).Should().Be.True();
			d.IsInAllowedDir(_TfPath.Combine("hej.txt")).Should().Be.True();
		}

		[Test]
		public void True_ForRelative()
		{
			var d = GetDirectoryAdapter();

			d.IsInAllowedDir("DirectoryAdapter_ChJail/hej")
				.Should("It should return true for relative paths.").Be.True();
			d.IsInAllowedDir("DirectoryAdapter_ChJail/hej.txt")
				.Should("It should return true for relative paths").Be.True();
		}

		private DirectoryAdapter GetDirectoryAdapter()
		{
			return new DirectoryAdapter(new MapPathImpl(), true, _TfPath);
		}

		[Test]
		public void IsInAllowedDirReturnsTrueIfNoConstraint()
		{
			var d = new DirectoryAdapter(new MapPathImpl(), false, null);
			Assert.IsTrue(d.IsInAllowedDir("\\"));
		}
	}
}