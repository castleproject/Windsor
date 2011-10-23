// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.IO.Tests.chjail
{
	using System;

	using Castle.IO.Extensions;

	using NUnit.Framework;

	using SharpTestsEx;

	public class can_chjail_absolute_path_spec
	{
		private Path an_absolute_path;
		private Path a_relative_path;
		private Path a_relative_path_with_drive;
		private Path a_relative_path_with_drive2;

		[SetUp]
		public void given_different_methods_of_saying_here_in_this_folder()
		{
			an_absolute_path = Environment.CurrentDirectory.ToPath().Combine("chjailing");
			a_relative_path = new Path(".").Combine("chjailing");
			a_relative_path_with_drive = new Path("C:./").Combine("chjailing");
			a_relative_path_with_drive2 = new Path("C:.").Combine("chjailing");
		}

		[Test]
		[TestCase("\\", "it's the root of the current drive")]
		[TestCase("\\\\?\\C:\\", "it's the root of the current drive as well")]
		[TestCase(@"\\.\dev0", "one can't write directory to a device")]
		[TestCase(@"\\?\UNC\/", "it's the root of the current drive")]
		[TestCase("../", "it's the parent of a_path, so it's not allowed")]
		[TestCase("..\\", "it's the parent of a_path, so it's not allowed")]
		[TestCase("C:..\\", "it's the parent of a_path (with drive letter), so it's not allowed")]
		[TestCase("\\", "it's the root of the file system")]
		[TestCase("/", "it's the root of the file system")]
		[TestCase("C:\\", "it's the root of the file system, with qualified harddrive")]
		public void disallowed_directories(string other_directory, string because)
		{
			an_absolute_path.AllowedToAccess(other_directory).Should(because).Be.False();
			a_relative_path.AllowedToAccess(other_directory).Should(because).Be.False();
			a_relative_path_with_drive.AllowedToAccess(other_directory).Should(because).Be.False();
			a_relative_path_with_drive2.AllowedToAccess(other_directory).Should(because).Be.False();
		}

		[Test]
		[TestCase("./")]
		[TestCase("C:./")]
		[TestCase("D:./")]
		[TestCase("D:./a")]
		[TestCase("D:./a/b")]
		[TestCase("D:./a/b/c.dir")]
		[TestCase("D:./a/b/.git")]
		[TestCase("./a")]
		[TestCase("./a/b")]
		[TestCase("./a/b/")]
		[TestCase("./a/b/c.dir")]
		[TestCase("./a/b/c/.git")]
		[TestCase("a")]
		[TestCase("a.dir")]
		[TestCase("a.dir.old")]
		[TestCase("a/b")]
		[TestCase("a/b/")]
		[TestCase("a/b/c.dir")]
		[TestCase("a/b/c/.git")]
		public void allowed_directories(string other_directory)
		{
			var reason = "because it's relative to this path";
			an_absolute_path.AllowedToAccess(other_directory).Should(reason).Be.True();
			a_relative_path.AllowedToAccess(other_directory).Should(reason).Be.True();
			a_relative_path_with_drive.AllowedToAccess(other_directory).Should(reason).Be.True();
			a_relative_path_with_drive2.AllowedToAccess(other_directory).Should(reason).Be.True();
		}
	}
}