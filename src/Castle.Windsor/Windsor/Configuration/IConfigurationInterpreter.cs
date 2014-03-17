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

namespace Castle.Windsor.Configuration
{
	using Castle.Core.Resource;
	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Configuration;

	/// <summary>
	///   Interpreter of a specific language to describe 
	///   configuration nodes in a hierarchical manner.
	/// </summary>
	public interface IConfigurationInterpreter
	{
		/// <summary>
		///   Gets or sets the name of the environment.
		/// </summary>
		/// <value>The name of the environment.</value>
		string EnvironmentName { get; set; }

		/// <summary>
		///   Exposes the reference to <see cref = "IResource" />
		///   which the interpreter is likely to hold
		/// </summary>
		IResource Source { get; }

		/// <summary>
		///   Should obtain the contents from the resource,
		///   interpret it and populate the <see cref = "IConfigurationStore" />
		///   accordingly.
		/// </summary>
		/// <param name = "resource"></param>
		/// <param name = "store"></param>
		/// <param name = "kernel"></param>
		void ProcessResource(IResource resource, IConfigurationStore store, IKernel kernel);
	}
}