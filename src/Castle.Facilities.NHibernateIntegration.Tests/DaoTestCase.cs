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

namespace Castle.Facilities.NHibernateIntegration.Tests.Common
{
	using System.Collections;
	using MicroKernel.Registration;
	using NUnit.Framework;

	[TestFixture]
	public class DaoTestCase : AbstractNHibernateTestCase
	{
		[Test]
		public void CommonUsage()
		{
			container.Register(Component.For<BlogDao>().Named("blogdao"));

			BlogDao dao = container.Resolve<BlogDao>("blogdao");
			dao.CreateBlog("my blog");

			IList blogs = dao.ObtainBlogs();

			Assert.IsNotNull(blogs);
			Assert.AreEqual(1, blogs.Count);
		}

		[Test]
		public void CommonStatelessUsage()
		{
			container.Register(Component.For<BlogDao>().Named("blogdao"));

			BlogDao dao = container.Resolve<BlogDao>("blogdao");
			dao.CreateBlog("my blog");

			IList blogs = dao.ObtainBlogsStateless();

			Assert.IsNotNull(blogs);
			Assert.AreEqual(1, blogs.Count);
		}
	}
}