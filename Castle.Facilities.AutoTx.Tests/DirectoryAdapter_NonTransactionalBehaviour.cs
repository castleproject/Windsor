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

using Castle.Facilities.Transactions.IO;
using Castle.Facilities.Transactions.Tests.Framework;
using NUnit.Framework;

namespace Castle.Facilities.Transactions.Tests
{
	using Facilities.Transactions.Tests.TestClasses;

	public class DirectoryAdapter_NonTransactionalBehaviour : TxFTestFixtureBase
	{
		[Test]
		public void Exists()
		{
			Assert.That(Directory.Exists("."));
		}

		[Test]
		public void Create_Then_Exists()
		{
			try
			{
				Assert.That(Directory.Create("tmp-xxx"), Is.True);
				Assert.That(Directory.Exists("tmp-xxx"));
			}
			finally
			{
				Directory.Delete("tmp-xxx");
			}
		}

		[Test]
		public void Create_Then_Delete()
		{
			Directory.Create("tmp-2");
			
			Assert.IsTrue(Directory.Exists("tmp-2"));
			
			Directory.Delete("tmp-2");

			Assert.IsFalse(Directory.Exists("tmp-2"));
		}

		[Test, Ignore("fix overwrite flag")]
		public void Move()
		{
			Directory.Create("tmp-3");

			File.WriteAllText("tmp-3".Combine("mytxt.txt"), "My Contents");

			Directory.Move("tmp-3", "tmp-3-moved", true);

			Assert.That(Directory.Exists("tmp-3-moved"));
			Assert.That(Directory.Exists("tmp-3"), Is.False);
			Assert.That(File.ReadAllText("tmp-3-moved".Combine("mytxt.txt")), Is.EqualTo("My Contents"));
		}
	}
}