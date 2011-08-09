#region license

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

#endregion

using Castle.IO.Extensions;
using Castle.Services.Transaction.Tests.Framework;
using Castle.Transactions.IO;
using NUnit.Framework;
using SharpTestsEx;

namespace Castle.Services.Transaction.Tests.Directories
{
	public class DirectoryAdapter_NonTransactionalBehaviour : TransactionManager_SpecsBase
	{
		[Test]
		public void Exists_Relative()
		{
			Directory.Exists(".").Should().Be.True();
			Directory.Exists("..").Should().Be.True();
			Directory.Exists("../").Should().Be.True();
		}

		[Test]
		public void Create_Then_Exists()
		{
			try
			{
				Directory.Create("tmp-Create_Then_Exists")
					.Should().Be.True();
				Directory.Exists("tmp-Create_Then_Exists")
					.Should().Be.True();
			}
			finally
			{
				Directory.Delete("tmp-Create_Then_Exists");
			}
		}

		[Test]
		public void Create_Then_Delete()
		{
			Directory.Create("tmp-Create_Then_Delete");
			
			Directory.Exists("tmp-Create_Then_Delete")
				.Should().Be.True();

			Directory.Delete("tmp-Create_Then_Delete");
			
			Directory.Exists("tmp-Create_Then_Delete")
				.Should().Be.False();
		}

		[Test]
		public void Move_Recursively()
		{
			// given
			Directory.Create("tmp-3");
			File.WriteAllText("tmp-3".Combine("mytxt.txt"), "My Contents");

			try
			{
				// when
				Directory.Move("tmp-3", "tmp-3-moved", true);

				// then:
				Directory.Exists("tmp-3-moved")
					.Should().Be.True();

				Directory.Exists("tmp-3")
					.Should().Be.False();

				File.ReadAllText("tmp-3-moved".Combine("mytxt.txt"))
					.Should().Be.EqualTo("My Contents");
			}
			finally
			{
				Directory.Delete("tmp-3-moved", true);
			}
		}
	}
}