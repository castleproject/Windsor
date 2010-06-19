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

namespace Castle.Facilities.Startable
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.SubSystems.Conversion;

	public class StartableContributor : IContributeComponentModelConstruction
	{
		private readonly ITypeConverter converter;

		public StartableContributor(ITypeConverter converter)
		{
			this.converter = converter;
		}

		private bool HasStartableAttributeSet(ComponentModel model)
		{
			if (model.Configuration == null)
			{
				return false;
			}

			var startable = model.Configuration.Attributes["startable"];
			if (startable != null)
			{
				return (bool)converter.PerformConversion(startable, typeof(bool));
			}

			return false;
		}

		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			var startable = CheckIfComponentImplementsIStartable(model)
			                || HasStartableAttributeSet(model);

			model.ExtendedProperties["startable"] = startable;

			if (startable)
			{

				AddStart(model);
				AddStop(model);
			}
		}

		private void AddStop(ComponentModel model)
		{
			var startMethod = model.Configuration.Attributes["stopMethod"];
			if (startMethod != null)
			{
				var method = model.Implementation.GetMethod(startMethod, Type.EmptyTypes);
				if (method == null)
				{
					throw new ArgumentException(
						"Could not find public parameterless method '{}' on type {1} designated as stop method. Make sure you didn't mistype the method name and that its signature matches.");
				}
				model.ExtendedProperties.Add("Castle.StartableFacility.StopMethod", method);
			}
			model.LifecycleSteps.AddFirst(LifecycleStepType.Decommission, StopConcern.Instance);
		}

		private void AddStart(ComponentModel model)
		{
			var startMethod = model.Configuration.Attributes["startMethod"];
			if (startMethod != null)
			{
				var method = model.Implementation.GetMethod(startMethod, Type.EmptyTypes);
				if(method == null)
				{
					throw new ArgumentException(
						"Could not find public parameterless method '{}' on type {1} designated as start method. Make sure you didn't mistype the method name and that its signature matches.");
				}
				model.ExtendedProperties.Add("Castle.StartableFacility.StartMethod", method);
			}
			model.LifecycleSteps.Add(LifecycleStepType.Commission, StartConcern.Instance);
		}

		private static bool CheckIfComponentImplementsIStartable(ComponentModel model)
		{
			return typeof(IStartable).IsAssignableFrom(model.Implementation);
		}
	}
}