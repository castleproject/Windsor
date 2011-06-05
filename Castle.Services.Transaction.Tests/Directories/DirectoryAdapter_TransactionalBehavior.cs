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
using Castle.IO.Internal;
using Castle.Services.Transaction.Tests.Framework;
using Castle.Services.Transaction.Tests.TestClasses;
using Castle.Transactions;
using NUnit.Framework;
using SharpTestsEx;

namespace Castle.Services.Transaction.Tests.Directories
{
	public class DirectoryAdapter_TransactionalBehavior : TxFTestFixtureBase
	{
		// have a look at http://msdn.microsoft.com/en-us/library/aa964931%28v=VS.85%29.aspx and see what can be done

		private string _TfPath;

		[SetUp]
		public void TFSetup()
		{
			var dllPath = Environment.CurrentDirectory;
			_TfPath = dllPath.CombineAssert("DirectoryAdapter_TransactionalBehavior");
		}

		[TearDown]
		public void TFTearDown()
		{
			if (_TfPath != null)
				Directory.Delete(_TfPath, true);
		}

		[Test]
		public void Smoke()
		{
		}

		[Test]
		public void Transacted_Create()
		{
			var dir = _TfPath.Combine("Transacted_Create");
			try
			{
				Create(dir, () => { ; });
			}
			finally
			{
				Directory.Delete(dir);
			}
		}

		[Test]
		public void Transacted_Exists()
		{
			var dir = _TfPath.Combine("Transacted_Exists");
			try
			{
				Create(dir, () => Directory.Exists(dir));
			}
			finally
			{
				Directory.Delete(dir);
			}
		}

		private void Create(string dir, Action a)
		{
			using (var ft = CreateTx())
			{
				Directory.Create(dir)
					.Should()
					.Be.True();
				a();
				ft.Complete();
			}
		}

		private ITransaction CreateTx()
		{
			return Manager.CreateFileTransaction().Value.Transaction;
		}
	}
}