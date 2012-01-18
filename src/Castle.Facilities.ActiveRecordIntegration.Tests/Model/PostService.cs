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

namespace Castle.Facilities.ActiveRecordIntegration.Tests.Model
{
	using System;

	using Castle.Services.Transaction;


	[Transactional]
	public class PostService
	{
		[Transaction(TransactionMode.Requires)]
		public virtual Post Create(Blog blog, String title, String contents, String category)
		{
			Post post = new Post(blog, title, contents, category);

			post.Save();

			return post;
		}

		[Transaction(TransactionMode.Requires)]
		public virtual Post CreateAndThrowException(Blog blog, String title, String contents, String category)
		{
			Post post = new Post(blog, title, contents, category);

			post.Save();

			throw new Exception("Dohhh!");
		}
	}
}
