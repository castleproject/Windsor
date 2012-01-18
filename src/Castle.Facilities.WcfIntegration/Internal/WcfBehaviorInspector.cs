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

namespace Castle.Facilities.WcfIntegration.Internal
{
	using System;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ModelBuilder;

	public class WcfBehaviorInspector : IContributeComponentModelConstruction
	{
		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (IsBehavior(model.Implementation))
			{
				UpdateLifestyle(model);
				UpdateActivator(model);
			}
		}

		private static bool IsBehavior(Type implementation)
		{
			return typeof(IServiceBehavior).IsAssignableFrom(implementation) ||
			       typeof(IEndpointBehavior).IsAssignableFrom(implementation) ||
			       typeof(IOperationBehavior).IsAssignableFrom(implementation) ||
			       typeof(IContractBehavior).IsAssignableFrom(implementation) ||
			       typeof(IErrorHandler).IsAssignableFrom(implementation);
		}

		private static void UpdateActivator(ComponentModel model)
		{
			if (model.CustomComponentActivator == null)
			{
				model.CustomComponentActivator = typeof(WcfBehaviorActivator);
			}
		}

		private static void UpdateLifestyle(ComponentModel model)
		{
			if (model.LifestyleType == LifestyleType.Undefined)
			{
				model.LifestyleType = LifestyleType.Transient;
			}
		}
	}
}