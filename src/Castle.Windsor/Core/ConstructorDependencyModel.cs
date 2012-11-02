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

namespace Castle.Core
{
	using System;
	using System.Reflection;

	using Castle.Core.Internal;

	[Serializable]
	public class ConstructorDependencyModel : DependencyModel
	{
		private ConstructorCandidate constructor;

		public ConstructorDependencyModel(ParameterInfo parameter)
			: base(parameter.Name, parameter.ParameterType, false, parameter.HasDefaultValue(), parameter.DefaultValue)
		{
		}

		public ConstructorCandidate Constructor
		{
			get { return constructor; }
		}

		internal void SetParentConstructor(ConstructorCandidate ctor)
		{
			constructor = ctor;
		}
	}
}