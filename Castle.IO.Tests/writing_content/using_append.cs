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
using NUnit.Framework;

namespace Castle.IO.Tests.writing_content
{
	[TestFixture(typeof (TestInMemoryFileSystem))]
	[TestFixture(typeof (TestLocalFileSystem))]
	public class using_append<T> : file_system_ctxt<T> where T : IFileSystem, new()
	{
		public using_append()
		{
			file = given_temp_file();
			given_content(1);
			given_content(42);
		}

		private void given_content(byte data)
		{
			file.Write(data, mode: FileMode.Append);
		}

		[Test]
		public void file_length_is_updated()
		{
			file.GetSize().ShouldBe(2);
		}

		[Test]
		public void file_content_is_written()
		{
			file.ShouldBe(1, 42);
		}

		[TestFixtureTearDown]
		public void DisposeFile()
		{
			if (file != null)
				file.Dispose();
		}

		private readonly ITemporaryFile file;
	}
}