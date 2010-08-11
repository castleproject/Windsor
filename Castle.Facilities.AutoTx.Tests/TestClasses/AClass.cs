#region License
//  Copyright 2004-2010 Castle Project - http:www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http:www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion

namespace Castle.Facilities.AutoTx.Tests
{
	using NUnit.Framework;
	using Services.Transaction;
	using Services.Transaction.IO;

	[Transactional]
	public class AClass : ISomething
	{
		private readonly IDirectoryAdapter _Da;
		private readonly IFileAdapter _Fa;

		public AClass(IDirectoryAdapter da, IFileAdapter fa)
		{
			_Da = da;
			_Fa = fa;
		}

		public IDirectoryAdapter Da
		{
			get { return _Da; }
		}

		public IFileAdapter Fa
		{
			get { return _Fa; }
		}

		[Transaction]
		public void A(ITransaction tx)
		{
			Assert.That(tx, Is.Null);
		}

		[Transaction, InjectTransaction]
		public void B(ITransaction tx)
		{
			Assert.That(tx, Is.Not.Null);
		}
	}
}