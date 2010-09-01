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

namespace Castle.Windsor.Tests
{
	using System;
	using System.Collections.Generic;

	using Castle.Windsor.Tests.Components;

	public interface IRepository<T>
	{
		T Get(int id);
	}

	//[Castle.Core.Transient] //Test passes if this attributed added
	public class RepositoryNotMarkedAsTransient<T> : IRepository<T> where T : new()
	{
		public T Get(int id)
		{
			return new T();
		}
	}

	public class DemoRepository<T> : IRepository<T>
	{
		public ICache<T> Cache { get; set; }

		public string Name { get; set; }

		public T Get(int id)
		{
			return Activator.CreateInstance<T>();
		}
	}

	public class ReviewerRepository : DemoRepository<IReviewer>
	{
		public new ICache<IReviewer> Cache { get; set; }

		public new string Name { get; set; }

		public new IReviewer Get(int id)
		{
			return null;
		}
	}

	public interface ICache<T>
	{
		T Get(string key);

		void Put(string key, T item);
	}

	public class DictionaryCache<T> : ICache<T>
	{
		private Dictionary<string, object> hash = new Dictionary<string, object>();

		public T Get(string key)
		{
			return (T)hash[key];
		}

		public void Put(string key, T item)
		{
			hash[key] = item;
		}
	}

	public class NullCache<T> : ICache<T>
	{
		public T Get(string key)
		{
			return default(T);
		}

		public void Put(string key, T item)
		{
		}
	}

	public class LoggingRepositoryDecorator<T> : IRepository<T>
	{
		public IRepository<T> inner;

		public LoggingRepositoryDecorator()
		{
		}

		public LoggingRepositoryDecorator(IRepository<T> inner)
		{
			this.inner = inner;
		}

		public T Get(int id)
		{
			Console.WriteLine("Getting {0}", id);
			return inner.Get(id);
		}
	}

	[Castle.Core.Transient]
	public class TransientRepository<T> : IRepository<T> where T : new()
	{
		public T Get(int id)
		{
			return new T();
		}
	}

	public class NeedsGenericType
	{
		private ICache<string> cache;

		public NeedsGenericType(ICache<string> cache)
		{
			this.cache = cache;
		}
	}
}