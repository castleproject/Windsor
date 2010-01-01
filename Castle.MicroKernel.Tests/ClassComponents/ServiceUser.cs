// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Tests.ClassComponents
{
	using System;

	public class ServiceUser
	{
		private A _a;
		private B _b;
		private C _c;

		public ServiceUser(A a)
		{
			if (a == null) throw new ArgumentNullException();
			_a = a;
		}

		public ServiceUser(A a, B b) : this(a)
		{
			if (b == null) throw new ArgumentNullException();
			_b = b;
		}

		public ServiceUser(A a, B b, C c) : this(a, b)
		{
			if (c == null) throw new ArgumentNullException();
			_c = c;
		}

		public A AComponent
		{
			get { return _a; }
		}

		public B BComponent
		{
			get { return _b; }
		}

		public C CComponent
		{
			get { return _c; }
		}
	}
}