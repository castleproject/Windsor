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

	public class SecondDao2
	{
		private readonly ISessionManager sessManager;

		public SecondDao2(ISessionManager sessManager)
		{
			this.sessManager = sessManager;
		}

		public BlogItem Create(Blog blog)
		{
			using (ISession session = sessManager.OpenSession())
			{
				BlogItem item = new BlogItem();

				item.ParentBlog = blog;
				item.ItemDate = DateTime.Now;
				item.Text = "x";
				item.Title = "splinter cell is cool!";

				session.Save(item);

				return item;
			}
		}

		public BlogItem CreateStateless(Blog blog)
		{
			using (IStatelessSession session = sessManager.OpenStatelessSession())
			{
				BlogItem item = new BlogItem();

				item.ParentBlog = blog;
				item.ItemDate = DateTime.Now;
				item.Text = "x";
				item.Title = "splinter cell is cool!";

				session.Insert(item);

				return item;
			}
		}
	}
}