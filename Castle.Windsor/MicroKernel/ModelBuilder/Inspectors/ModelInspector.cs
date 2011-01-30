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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Registration;

	public class ModelInspector<T> : IContributeComponentModelConstruction where T : class
	{
		private readonly List<ComponentDescriptor<T>> descriptors;

		public ModelInspector(List<ComponentDescriptor<T>> descriptors)
		{
			this.descriptors = descriptors;
		}

		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			descriptors.ForEach(d => d.ApplyToModel(kernel, model));
		}
	}
}