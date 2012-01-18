#region License

//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

#endregion

namespace Castle.Facilities.NHibernateIntegration.Tests.Transactions
{
	using System;
	using NHibernateIntegration.Components.Dao;
	using Services.Transaction;

	[Transactional]
	public class RootService2 : NHibernateGenericDao
	{
		private readonly FirstDao2 firstDao;
		private readonly SecondDao2 secondDao;
		private OrderDao2 orderDao;

		public RootService2(FirstDao2 firstDao, SecondDao2 secondDao, ISessionManager sessManager)
			: base(sessManager)
		{
			this.firstDao = firstDao;
			this.secondDao = secondDao;
		}

		public OrderDao2 OrderDao
		{
			get { return orderDao; }
			set { orderDao = value; }
		}

		[Transaction(Distributed = true)]
		public virtual void DoTwoDBOperation_Create(bool throwException)
		{
			Blog blog = firstDao.Create();
			secondDao.Create(blog);
			orderDao.Create(1.122f);

			if (throwException)
			{
				throw new InvalidOperationException("Nah, giving up");
			}
		}

		[Transaction(Distributed = true)]
		public virtual void DoTwoDBOperation_Create_Stateless(bool throwException)
		{
			Blog blog = firstDao.CreateStateless();
			secondDao.CreateStateless(blog);
			orderDao.CreateStateless(1.122f);

			if (throwException)
			{
				throw new InvalidOperationException("Nah, giving up");
			}
		}
	}
}