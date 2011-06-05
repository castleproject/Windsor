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

using System.Linq;
using Castle.IO.Tests.contexts;
using NUnit.Framework;

namespace Castle.IO.Tests.directory_searches
{
	[TestFixture(typeof (TestInMemoryFileSystem))]
	[TestFixture(typeof (TestLocalFileSystem))]
	public class search_for_absolute_paths<T> : files<T> where T : IFileSystem, new()
	{
		private const string TEMPFOLDER = "CBB7E871-89FF-4F20-A58E-73EB4D2F1191";

		[TestCase(@"c:\" + TEMPFOLDER + @"\test\", @"c:\" + TEMPFOLDER + @"\*")]
		[TestCase(@"c:\" + TEMPFOLDER + @"\test\", @"c:\" + TEMPFOLDER + @"\test\")]
		[TestCase(@"c:\" + TEMPFOLDER + @"\test\", @"c:\" + TEMPFOLDER + @"\test")]
		[TestCase(@"c:\" + TEMPFOLDER + "\\", @"c:\" + TEMPFOLDER)]
		public void finds_directory(string directoryPath, string searchString)
		{
			IDirectory folder = null;
			try
			{
				folder = FileSystem.GetDirectory(directoryPath).MustExist();

				FileSystem.GetCurrentDirectory().Directories(searchString, SearchScope.CurrentOnly)
					.ShouldHaveCountOf(1)
					.First().Path.FullPath.ShouldBe(directoryPath);
			}
			finally
			{
				folder.Delete();
			}
		}
	}
}