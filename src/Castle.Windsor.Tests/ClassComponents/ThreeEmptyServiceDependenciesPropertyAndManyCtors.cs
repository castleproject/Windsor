// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using CastleTests.Components;

	public class ThreeEmptyServiceDependenciesPropertyAndManyCtors
	{
		public ThreeEmptyServiceDependenciesPropertyAndManyCtors(IEmptyService one)
		{
			One = one;
		}

		public ThreeEmptyServiceDependenciesPropertyAndManyCtors(IEmptyService one, IDoubleGeneric<int, A> two)
		{
			One = one;
			Two = two;
		}

		public ThreeEmptyServiceDependenciesPropertyAndManyCtors(IEmptyService one, IEmptyService two, IEmptyService three)
		{
			One = one;
			Two = two;
			Three = three;
		}

		public IEmptyService One { get; set; }
		public IEmptyService Three { get; set; }
		public object Two { get; set; }
	}
}