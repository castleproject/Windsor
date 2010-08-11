#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

namespace Castle.Services.Transaction.Tests
{
	using System;
	using IO;
	using NUnit.Framework;

	[TestFixture]
	public class FileAdapterTests
	{
		[Test]
		public void CtorUseTransactions()
		{
			Assert.That(new FileAdapter().UseTransactions);
		}
	}


	[TestFixture]
	public class DirectoryAdapterTests
	{
		private string curr_dir;

		[TestFixtureSetUp]
		public void SetUp()
		{
			curr_dir = Path.GetPathWithoutLastBit(Path.GetFullPath(typeof (DirectoryAdapterTests).Assembly.CodeBase));
		}

		[Test]
		public void CtorWorksIfNullAndNotConstaint()
		{
			var adapter = new DirectoryAdapter(new MapPathImpl(), false, null);
			Assert.That(adapter.UseTransactions);
		}

//		[Test]
//		public void IsInAllowedDir_ReturnsFalseIfConstaint_AndOutside()
//		{
//			var d = new DirectoryAdapter(new MapPathImpl(), true, curr_dir);
//
//			Assert.IsFalse(d.IsInAllowedDir("\\"));
//			Assert.IsFalse(d.IsInAllowedDir("\\\\?\\C:\\"));
//			Assert.IsFalse(d.IsInAllowedDir(@"\\.\dev0"));
//			Assert.IsFalse(d.IsInAllowedDir(@"\\?\UNC\/"));
//		}

		[Test]
		public void CanGetLocalFile()
		{
			// "C:\Users\xyz\Documents\dev\logibit_cms\scm\trunk\Tests\Henrik.Cms.Tests\TestGlobals.cs";
			var d = new DirectoryAdapter(new MapPathImpl(), false, null);
			string path = Path.GetPathWithoutLastBit(d.MapPath("~/../../TestGlobals.cs")); // get directory instead
			Console.WriteLine(path);
			Assert.That(d.Exists(path));
		}

//		[Test]
//		public void IsInAllowedDir_ReturnsTrueForInside()
//		{
//			var d = new DirectoryAdapter(new MapPathImpl(), true, curr_dir);
//			Assert.IsTrue(d.IsInAllowedDir(curr_dir));
//			Assert.IsTrue(d.IsInAllowedDir(curr_dir.Combine("hej/something/test")));
//			Assert.IsTrue(d.IsInAllowedDir(curr_dir.Combine("hej")));
//			Assert.IsTrue(d.IsInAllowedDir(curr_dir.Combine("hej.txt")));
//
//			Assert.IsTrue(d.IsInAllowedDir("hej"), "It should return true for relative paths.");
//			Assert.IsTrue(d.IsInAllowedDir("hej.txt"), "It should return true for relative paths");
//		}
//
//		[Test]
//		public void IsInAllowedDirReturnsTrueIfNoConstraint()
//		{
//			var ad = new DirectoryAdapter(new MapPathImpl(), false, null);
//			Assert.IsTrue(ad.IsInAllowedDir("\\"));
//		}
	}
}