// Copyright 2004-2024 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Components
{
	public interface IHasState<T>
	{
		IState<T> State { get; set; }
	}

	public interface IState<T>
	{
	}

	/// <summary>
	///   A component that has a property dependency on <see cref="IState{T}"/>.
	///   When registered as singleton, resolving this component will trigger a
	///   property injection cycle if the <see cref="IState{T}"/> implementation
	///   depends back on <see cref="IHasState{T}"/>.
	/// </summary>
	public class PublisherWithState<T> : IHasState<T>
	{
		public IState<T> State { get; set; }
	}

	/// <summary>
	///   A component that takes <see cref="IHasState{T}"/> as a constructor
	///   dependency, forming the other half of a cycle with
	///   <see cref="PublisherWithState{T}"/>.
	/// </summary>
	public class StateComponent<T> : IState<T>
	{
		public StateComponent(IHasState<T> publisher)
		{
			Publisher = publisher;
		}

		public IHasState<T> Publisher { get; }
	}
}
