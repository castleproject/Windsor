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
using NUnit.Framework;

namespace Castle.IO.Tests.directory_searches
{
	[TestFixture("c:\\path\\folder\\", "c:\\**\\folder")]
	[TestFixture("c:\\path\\folder\\", "c:\\path\\**\\folder")]
	[TestFixture("c:\\path\\folder\\", "c:\\path\\**\\**\\folder")]
	[TestFixture("c:\\path\\folder\\", "c:\\p*\\f*")]
	[TestFixture("c:\\path\\folder\\", "c:\\path\\f*")]
	[TestFixture("c:\\path\\folder\\", "**\\folder")]
	[TestFixture("c:\\path\\folder\\", "path\\**\\folder")]
	[TestFixture("c:\\path\\folder\\", "path\\**\\**\\folder")]
	[TestFixture("c:\\path\\folder\\", "p*\\f*")]
	[TestFixture("c:\\path\\folder\\", "path\\f*")]
	[TestFixture("c:\\path\\folder\\", "*\\f*")]
	[TestFixture("c:\\path\\folder\\", "path\\**\\*")]
	[TestFixture("c:\\path\\folder\\", "c:\\path\\folder\\")]
	public class recursive_directory_search : file_search_context
	{
		private readonly string _existingDirectory;

		public recursive_directory_search(string directory, string searchSpec)
		{
			_existingDirectory = directory;
			given_directory(directory);

			when_searching_for_directories(searchSpec);
		}

		[Test]
		public void file_is_found()
		{
			Directories.ShouldHaveCountOf(1)
				.First().Path.FullPath
				.ShouldBe(_existingDirectory);
		}
	}
}