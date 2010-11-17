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

namespace Castle.MicroKernel.Context
{
	using System;
	using System.Collections.Generic;

	public interface IArgumentsStore : IEnumerable<KeyValuePair<object, object>>
	{
		bool Contains(object key);

		int Count { get; }

		bool Supports(Type keyType);

		void Add(object key, object value);

		void Clear();

		bool Remove(object key);

		bool Insert(object key, object value);

		object GetItem(object key);
	}
}