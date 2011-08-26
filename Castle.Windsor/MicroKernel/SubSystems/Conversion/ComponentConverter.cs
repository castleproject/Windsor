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

namespace Castle.MicroKernel.SubSystems.Conversion
{
	using System;

	using Castle.Core.Configuration;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Util;

	[Serializable]
	public class ComponentConverter : AbstractTypeConverter, IKernelDependentConverter
	{
		public override bool CanHandleType(Type type, IConfiguration configuration)
		{
			if (configuration.Value != null)
			{
				return ReferenceExpressionUtil.IsReference(configuration.Value);
			}
			return CanHandleType(type);
		}

		public override bool CanHandleType(Type type)
		{
			if (Context.Kernel == null)
			{
				return false;
			}

			return Context.Kernel.HasComponent(type);
		}

		public override object PerformConversion(String value, Type targetType)
		{
			var componentName = ReferenceExpressionUtil.ExtractComponentKey(value);
			if (componentName == null)
			{
				throw new ConverterException(string.Format("Could not convert expression '{0}' to type '{1}'. Expecting a reference override like ${{some key}}",
				                                           value,
				                                           targetType.FullName));
			}

			var handler = Context.Kernel.LoadHandlerByName(componentName, targetType, null);
			if (handler == null)
			{
				throw new ConverterException(string.Format("Component '{0}' was not found in the container.", componentName));
			}

			return handler.Resolve(Context.CurrentCreationContext ?? CreationContext.CreateEmpty());
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			return PerformConversion(configuration.Value, targetType);
		}
	}
}