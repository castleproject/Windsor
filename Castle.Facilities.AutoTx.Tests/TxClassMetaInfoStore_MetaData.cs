#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Linq;
using Castle.Facilities.AutoTx.Tests.Framework;
using Castle.Facilities.AutoTx.Tests.TestClasses;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	public class TxClassMetaInfoStore_MetaData
	{
	    [Test]
		public void CanGetTxClassMetaInfo()
		{
			var meta = TxClassMetaInfoStore.GetMetaFromTypeInner(typeof(MyService));
			meta.ShouldPass("MyService has Transaction attributes")
				.ShouldBe(m => m.TransactionalMethods.Count() >= 4, "there are two methods");
		}
	}
}