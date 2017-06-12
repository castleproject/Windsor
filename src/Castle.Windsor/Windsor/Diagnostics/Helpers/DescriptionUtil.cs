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

namespace Castle.Windsor.Diagnostics.Helpers
{
	using System.ComponentModel;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel;

	public static class DescriptionUtil
	{
		public static string GetComponentName(this IHandler handler)
		{
			var componentName = handler.ComponentModel.ComponentName;
			if (componentName.SetByUser)
			{
				return string.Format("\"{0}\" {1}", componentName.Name, handler.GetServicesDescription());
			}
			return handler.GetServicesDescription();
		}

		public static string GetLifestyleDescription(this ComponentModel componentModel)
		{
			if (componentModel.LifestyleType == LifestyleType.Undefined)
			{
				return string.Format("{0}*", LifestyleType.Singleton);
			}
			if (componentModel.LifestyleType != LifestyleType.Custom)
			{
				return componentModel.LifestyleType.ToString();
			}
			return componentModel.CustomLifestyle.Name;
		}

		public static string GetLifestyleDescriptionLong(this ComponentModel componentModel)
		{
			if (componentModel.LifestyleType == LifestyleType.Undefined)
			{
				return string.Format("{0} (default lifestyle {1} will be used)", componentModel.LifestyleType, LifestyleType.Singleton);
			}
			if (componentModel.LifestyleType == LifestyleType.Scoped)
			{
				var accessorType = componentModel.GetScopeAccessorType();
				if (accessorType == null)
				{
					return "Scoped explicitly";
				}
				var description = accessorType.GetAttribute<DescriptionAttribute>();
				if (description != null)
				{
					return "Scoped " + description.Description;
				}
				return "Scoped via " + accessorType.ToCSharpString();
			}
			if (componentModel.LifestyleType != LifestyleType.Custom)
			{
				return componentModel.LifestyleType.ToString();
			}
			return "Custom: " + componentModel.CustomLifestyle.Name;
		}

		public static string GetServicesDescription(this IHandler handler)
		{
			return handler.ComponentModel.ToString();
		}
	}
}