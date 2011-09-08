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
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Util;

	[Serializable]
	public class MixinInspector : IContributeComponentModelConstruction
	{
		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (model.Configuration == null)
			{
				return;
			}

			var mixins = model.Configuration.Children["mixins"];
			if (mixins == null)
			{
				return;
			}

			var mixinReferences = new List<ComponentReference<object>>();
			foreach (var mixin in mixins.Children)
			{
				var value = mixin.Value;

				var mixinComponent = ReferenceExpressionUtil.ExtractComponentName(value);
				if (mixinComponent == null)
				{
					throw new Exception(
						String.Format("The value for the mixin must be a reference to a component (Currently {0})", value));
				}

				mixinReferences.Add(new ComponentReference<object>("mixin-" + mixinComponent, mixinComponent));
			}
			if (mixinReferences.Count == 0)
			{
				return;
			}
			var options = model.ObtainProxyOptions();
			mixinReferences.ForEach(options.AddMixinReference);
		}
	}
}