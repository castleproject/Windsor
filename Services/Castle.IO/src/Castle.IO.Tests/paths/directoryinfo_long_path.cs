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

namespace Castle.IO.Tests.paths
{
	using System.Diagnostics.Contracts;
	using System.IO;

	using NUnit.Framework;

	public class directoryinfo_long_path
	{
		[Test]
		public void create_long_path()
		{
			var longPath = LongPath(259);
			//Console.WriteLine("Path length: {0}", longPath.Length);
			var info = new DirectoryInfo(longPath);
			info.Exists.ShouldBeFalse(); // 260 chars is the exclusive upper limit here.
		}

		[Test]
		public void create_too_long_path()
		{
			var longPath = LongPath(260);
			Assert.Throws<PathTooLongException>(() => new DirectoryInfo(longPath).Exists.ShouldBeFalse());
		}

		[Pure]
		private static string LongPath(int length)
		{
			return string.Format(@"\\l{0}ng\share", new string('o', length - 11));
		}
	}
}