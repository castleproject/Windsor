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

namespace Castle.Windsor.Experimental.Debugging.Primitives
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel;

	public class MetaComponent
	{
		private readonly IList<Type> forwardedTypes;

		public MetaComponent(string name, IHandler handler, IList<Type> forwardedTypes)
		{
			Name = name;
			Handler = handler;
			this.forwardedTypes = forwardedTypes;
		}

		public IEnumerable<Type> ForwardedTypes
		{
			get { return forwardedTypes; }
		}

		public int ForwardedTypesCount
		{
			get { return forwardedTypes.Count; }
		}

		public IHandler Handler { get; private set; }

		public ComponentModel Model
		{
			get { return Handler.ComponentModel; }
		}

		public string Name { get; private set; }
	}
}