// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.ModelBuilder
{
	using Castle.Core;

	/// <summary>
	///   Implementors must inspect the component for 
	///   a given information or parameter.
	/// </summary>
	public interface IContributeComponentModelConstruction
	{
		/// <summary>
		///   Usually the implementation will look in the configuration property 
		///   of the model or the service interface, or the implementation looking for
		///   something.
		/// </summary>
		/// <param name = "kernel">The kernel instance</param>
		/// <param name = "model">The component model</param>
		void ProcessModel(IKernel kernel, ComponentModel model);
	}
}