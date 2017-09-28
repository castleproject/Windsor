// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Context
{
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel.Handlers;

	public static class CreationContextExtensions
	{
		public static void ApplyTo(this CreationContext context, object instance)
		{
			// We need to inject this to minimise the madness around all the custom extensions of castle windsor in facilities, 
			// this is mainly for typed factories for now but I hope that I can start reducing indirection and diversion from 
			// base behaviour in the microkernel in the future. For now we need this for downstream management of lifestyles.
			// I am really not happy about the way the ITypedFactoryComponentSelector just goes off in it's own direction. 
			// It creates a callback spaghetti mess that is very hard to debug and almost impossible to read control flow
			// from code. 

			// I hope we can chat about this in the PR. https://github.com/castleproject/Windsor/pull/61#issuecomment-305029815

			var creationContexts = instance.GetType().GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == typeof(CreationContext)).ToList();
			if (creationContexts.Any())
			{
				creationContexts.First().SetValue(instance, context);
			}
		}

		public static void RemoveCreationContext(this object instance)
		{
			if (instance == null) return;
			var creationContexts = instance.GetType().GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == typeof(CreationContext)).ToList();
			if (creationContexts.Any())
			{
				creationContexts.First().SetValue(instance, null);
			}
		}

		public static LifestyleType GetLifestyleType(this CreationContext context)
		{
			if (context?.Handler is AbstractHandler)
			{
				return (context.Handler as AbstractHandler).ComponentModel.LifestyleType;
			}
			return LifestyleType.Undefined;
		}
	}
}