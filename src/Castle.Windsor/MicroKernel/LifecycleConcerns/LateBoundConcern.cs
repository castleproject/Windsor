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

namespace Castle.MicroKernel.LifecycleConcerns
{
	using System;
	using System.Collections.Generic;
	using Castle.Core;

	/// <summary>
	/// Lifetime concern that works for components that don't have their actual type determined upfront
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class LateBoundConcern : ILifecycleConcern
	{
		private IDictionary<Type, ILifecycleConcern> steps;

		public void AddStep<TForType>(ILifecycleConcern lifecycleConcern)
		{
			if (steps == null)
			{
				steps = new Dictionary<Type, ILifecycleConcern>();
			}
			steps.Add(typeof (TForType), lifecycleConcern);
		}

		public bool HasSteps
		{
			get { return steps != null; }
		}

		public void Apply(ComponentModel model, object component)
		{
			Type type = component.GetType();
			foreach (var step in steps)
			{
				if (step.Key.IsAssignableFrom(type))
				{
					step.Value.Apply(model, component);
				}
			}
		}
	}
}