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
	using NHibernate;
	using Services.Transaction;

	[Transactional]
	public class FirstDao
	{
		private readonly ISessionManager sessManager;

		public FirstDao(ISessionManager sessManager)
		{
			this.sessManager = sessManager;
		}

		[Transaction]
		public virtual Blog Create()
		{
			return Create("xbox blog");
		}

		[Transaction]
		public virtual Blog Create(String name)
		{
			NHibernate.ITransaction tran;

			using (ISession session = sessManager.OpenSession())
			{
				tran = session.Transaction;

				NUnit.Framework.Assert.IsNotNull(session.Transaction);
				NUnit.Framework.Assert.IsTrue(session.Transaction.IsActive);

				Blog blog = new Blog();
				blog.Name = name;
				session.Save(blog);
				return blog;
			}
		}

		[Transaction]
		public virtual void Delete(String name)
		{
			using (ISession session = sessManager.OpenSession())
			{
				NUnit.Framework.Assert.IsNotNull(session.Transaction);
				session.Delete("from Blog b where b.Name ='" + name + "'");
				session.Flush();
			}
		}


		public virtual void AddBlogRef(BlogRef blogRef)
		{
			using (ISession session = sessManager.OpenSession())
			{
				session.Save(blogRef);
			}
		}

		[Transaction]
		public virtual Blog CreateStateless()
		{
			return CreateStateless("xbox blog");
		}

		[Transaction]
		public virtual Blog CreateStateless(String name)
		{
			NHibernate.ITransaction tran;

			using (IStatelessSession session = sessManager.OpenStatelessSession())
			{
				tran = session.Transaction;

				NUnit.Framework.Assert.IsNotNull(session.Transaction);
				NUnit.Framework.Assert.IsTrue(session.Transaction.IsActive);

				Blog blog = new Blog();
				blog.Name = name;
				session.Insert(blog);
				return blog;
			}
		}

		[Transaction]
		public virtual void DeleteStateless(String name)
		{
			using (IStatelessSession session = sessManager.OpenStatelessSession())
			{
				NUnit.Framework.Assert.IsNotNull(session.Transaction);
				session.Delete("from Blog b where b.Name ='" + name + "'");
			}
		}


		public virtual void AddBlogRefStateless(BlogRef blogRef)
		{
			using (IStatelessSession session = sessManager.OpenStatelessSession())
			{
				session.Insert(blogRef);
			}
		}
	}
}