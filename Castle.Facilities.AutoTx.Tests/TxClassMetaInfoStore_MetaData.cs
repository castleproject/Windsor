#region license

// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

using System;
using System.Linq;
using System.Reflection;
using Castle.Facilities.AutoTx.Tests.Framework;
using Castle.Facilities.AutoTx.Tests.TestClasses;
using Castle.Services.Transaction;
using Castle.Services.Transaction.Internal;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class TxClassMetaInfoStore_MetaData
	{
		[Test, Explicit("Testing whether we can use GetHashCode to exclude methods from object")]
		public void Scratch()
		{
			typeof (object).GetMethods().Do(Console.WriteLine).Run(x => Console.WriteLine(x.GetHashCode()));
			Console.WriteLine();

			typeof (SubClass)
				.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				//.Where(x => !objectMethods.Contains(x.GetHashCode()))
				.Do(Console.WriteLine)
				.Do(x => Console.WriteLine(x.Name))
				.Run();
		}

		[Test]
		public void The_Meta_Store_Knows_Subclasses()
		{
			var meta = TransactionClassMetaInfoStore.GetMetaFromTypeInner(typeof (SubClass));
			meta.ShouldPass("SubClass has Transaction attributes")
				.ShouldBe(m => m.TransactionalMethods.Count() == 1, "One on the base class");
		}

		[Test]
		public void CanGetTxClassMetaInfo()
		{
			var meta = TransactionClassMetaInfoStore.GetMetaFromTypeInner(typeof (MyService));
			meta.ShouldPass("MyService has Transaction attributes")
				.ShouldBe(m => m.TransactionalMethods.Count() >= 4, "there are four or more methods");
		}
	}

	// testing the hierarchy; base class has transactional method!
	public class SubClass : BaseClass
	{
	}

	public abstract class BaseClass
	{
		[Transaction]
		public string Work()
		{
			return "Hello";
		}
	}
}