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

namespace Castle.Windsor.Experimental.Diagnostics.Helpers
{
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel;

#if !SILVERLIGHT
	public static class DescriptionUtil
	{
		public static string GetLifestyleDescription(this ComponentModel componentModel)
		{
			if (componentModel.LifestyleType != LifestyleType.Custom)
			{
				return componentModel.LifestyleType.ToString();
			}
			return string.Format("custom ({0})", componentModel.CustomLifestyle.FullName);
		}

		public static string GetServicesDescription(this IHandler handler)
		{
			var services = handler.Services.ToArray();
			var additionalServicesCount = services.Length - 1;
			var message = new StringBuilder(services[0].Name);
			if (additionalServicesCount == 1)
			{
				message.Append(" (and one more type)");
			}
			else if (additionalServicesCount > 1)
			{
				message.AppendFormat(" (and {0} more types)", additionalServicesCount);
			}
			var impl = handler.ComponentModel.Implementation;
			if (additionalServicesCount == 0 && impl == services[0])
			{
				return message.ToString();
			}
			message.Append(" / ");
			if (impl == null)
			{
				message.Append("no type");
			}
			else if (impl == typeof(LateBoundComponent))
			{
				message.Append("late bound type");
			}
			else
			{
				message.Append(impl.Name);
			}
			return message.ToString();
		}
	}
#endif
}