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

namespace Castle.Core.Internal
{
	using System;
	using System.Text;

	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;

	public static class HelpfulExceptionsUtil
	{
		public static Exception TrackInstanceCalledMultipleTimes(object instance, Burden burden)
		{
			var message = new StringBuilder();
			message.AppendLine("Instance " + instance + " of component " + burden.Model.Name + " is already being tracked.");
			if (burden.Model.CustomComponentActivator == null)
			{
				if (burden.Model.LifestyleType != LifestyleType.Custom)
				{
					// unlikely, but hey, who knows
					message.Append("This looks like a bug in Windsor. Please report it.");
				}
				else
				{
					//no custom activator case - meaning the lifestyle is trying to pass the same instance around multiple times
					message.Append("The custom lifestyle " + burden.Model.CustomLifestyle +
					                   " seems to be buggy (or not configured properly), trying to track the same instances multiple times. Please report this to the author of the lifestyle.");
				}
			}
			else if (IsUsingFactoryMethod(burden.Model))
			{
				// .UsingFactoryMethod case
				if (burden.Model.LifestyleType == LifestyleType.Transient)
				{
					message.AppendLine("The factory method providing instances of the component is reusing instances, but the lifestyle of the component is " + burden.Model.LifestyleType +
					                   " which requires new instance each time.");
				}
				else if (burden.Model.LifestyleType == LifestyleType.Custom)
				{
					message.AppendLine("The factory method providing instances of the component is reusing instances, in a way that the custom lifestyle of the component (" + burden.Model.LifestyleType +
					                   ") does not expect.");
				}
				else
				{
					message.AppendLine("The factory method providing instances of the component is reusing instances, in a way that the lifestyle of the component (" + burden.Model.LifestyleType +
					                   ") does not expect.");
				}
				message.AppendLine("In most cases it is advised for the factory method not to be handling reuse of the instances, but to chose a lifestyle that does that appropriately.");
				message.Append(
					"Alternatively, if you do not wish for Windsor to track the objects coming from the factory change your regustration to '.UsingFactoryMethod(yourFactory, managedExternally: true)'");
			}
			else
			{
				// some other custom activator
				if (burden.Model.LifestyleType == LifestyleType.Transient)
				{
					message.AppendLine("The custom activator"+burden.Model.CustomComponentActivator+" is reusing instances, but the lifestyle of the component is " + burden.Model.LifestyleType +
					                   " which requires new instance each time.");
				}
				else if (burden.Model.LifestyleType == LifestyleType.Custom)
				{
					message.AppendLine("The custom activator" + burden.Model.CustomComponentActivator + " is reusing instances, in a way that the custom lifestyle of the component (" + burden.Model.LifestyleType +
					                   ") does not expect.");
				}
				else
				{
					message.AppendLine("The custom activator" + burden.Model.CustomComponentActivator + " is reusing instances, in a way that the lifestyle of the component (" + burden.Model.LifestyleType +
					                   ") does not expect.");
				}
				message.Append("In most cases it is advised for the activator not to be handling reuse of the instances, but to chose a lifestyle that does that appropriately.");
			}
			return new ComponentActivatorException(message.ToString(), burden.Model);
		}

		private static bool IsUsingFactoryMethod(ComponentModel componentModel)
		{
			return componentModel.CustomComponentActivator != null &&
			       componentModel.CustomComponentActivator.IsGenericType &&
			       componentModel.CustomComponentActivator.GetGenericTypeDefinition() == typeof(FactoryMethodActivator<>);
		}
	}
}