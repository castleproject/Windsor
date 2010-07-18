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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;
	using System.ComponentModel;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.LifecycleConcerns;

	/// <summary>
	/// Inspects the type looking for interfaces that constitutes
	/// lifecycle interfaces, defined in the Castle.Model namespace.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class LifecycleModelInspector : IContributeComponentModelConstruction
	{
		/// <summary>
		/// Checks if the type implements <see cref="IInitializable"/> and or
		/// <see cref="IDisposable"/> interfaces.
		/// </summary>
		/// <param name="kernel"></param>
		/// <param name="model"></param>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}
			if (IsLateBoundComponent(model))
			{
				ProcessLateBoundModel(model);
				return;
			}
			ProcessModel(model);
		}

		private bool IsLateBoundComponent(ComponentModel model)
		{
			return typeof(LateBoundComponent) == model.Implementation;
		}

		private void ProcessLateBoundModel(ComponentModel model)
		{
			var lateBoundCommissionConcern = new LateBoundConcerns();
			if (model.Implementation.Is<IInitializable>())
			{
				model.Lifecycle.Add(InitializationConcern.Instance);
			}
			else
			{
				lateBoundCommissionConcern.AddConcern<IInitializable>(InitializationConcern.Instance);
			}
#if (!SILVERLIGHT)
			if (model.Implementation.Is<ISupportInitialize>())
			{
				model.Lifecycle.Add(SupportInitializeConcern.Instance);
			}
			else
			{
				lateBoundCommissionConcern.AddConcern<ISupportInitialize>(SupportInitializeConcern.Instance);
			}
#endif
			if (model.Implementation.Is<IDisposable>())
			{
				model.Lifecycle.Add(DisposalConcern.Instance);
			}
			else
			{
				var decommission = new LateBoundConcerns();
				decommission.AddConcern<IDisposable>(DisposalConcern.Instance);
				model.Lifecycle.Add(decommission as IDecommissionConcern);
			}
			if (lateBoundCommissionConcern.HasConcerns)
			{
				model.Lifecycle.Add(lateBoundCommissionConcern as ICommissionConcern);
			}
		}

		private void ProcessModel(ComponentModel model)
		{
			if (model.Implementation.Is<IInitializable>())
			{
				model.Lifecycle.Add(InitializationConcern.Instance);
			}
#if (!SILVERLIGHT)
			if (model.Implementation.Is<ISupportInitialize>())
			{
				model.Lifecycle.Add(SupportInitializeConcern.Instance);
			}
#endif
			if (model.Implementation.Is<IDisposable>())
			{
				model.Lifecycle.Add(DisposalConcern.Instance);
			}
		}
	}
}