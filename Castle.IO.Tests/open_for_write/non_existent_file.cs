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

using System.IO;
using Castle.IO.Tests.TestClasses;
using NUnit.Framework;

namespace Castle.IO.Tests.open_for_write
{
	[TestFixture(typeof (TestInMemoryFileSystem))]
	//[TestFixture(typeof (TestLocalFileSystem))]
	public class non_existent_file<T> : files<T> where T : IFileSystem, new()
	{
		public non_existent_file()
		{
			given_temp_dir();
		}

		[TestCase(FileMode.Append)]
		[TestCase(FileMode.Create)]
		[TestCase(FileMode.CreateNew)]
		[TestCase(FileMode.OpenOrCreate)]
		public void file_is_created_for_mode(FileMode mode)
		{
			var tempFile = write_to_file(mode: mode);

			tempFile.Exists.ShouldBeTrue();
		}

		[TestCase(FileMode.Open)]
		[TestCase(FileMode.Truncate)]
		public void error_is_throw_for_mode(FileMode mode)
		{
			Executing(() => write_to_file(mode: mode))
				.ShouldThrow<FileNotFoundException>();
		}
	}
}