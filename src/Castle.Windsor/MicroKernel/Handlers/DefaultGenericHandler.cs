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

namespace Castle.MicroKernel.Handlers
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Proxy;

	/// <summary>
	///   Summary description for DefaultGenericHandler.
	/// </summary>
	/// <remarks>
	///   TODO: Consider refactoring AbstractHandler moving lifestylemanager creation to DefaultHandler
	/// </remarks>
	[Serializable]
	public class DefaultGenericHandler : AbstractHandler
	{
		private readonly IGenericImplementationMatchingStrategy implementationMatchingStrategy;

		private readonly IDictionary<Type, IHandler> type2SubHandler = new Dictionary<Type, IHandler>();

		/// <summary>
		///   Initializes a new instance of the <see cref = "DefaultGenericHandler" /> class.
		/// </summary>
		/// <param name = "model"></param>
		/// <param name = "implementationMatchingStrategy"></param>
		public DefaultGenericHandler(ComponentModel model, IGenericImplementationMatchingStrategy implementationMatchingStrategy) : base(model)
		{
			this.implementationMatchingStrategy = implementationMatchingStrategy;
		}

		public IGenericImplementationMatchingStrategy ImplementationMatchingStrategy
		{
			get { return implementationMatchingStrategy; }
		}

		public override void Dispose()
		{
			foreach (var handler in type2SubHandler.Values)
			{
				var disposable = handler as IDisposable;
				if (disposable == null)
				{
					continue;
				}
				disposable.Dispose();
			}
			type2SubHandler.Clear();
		}

		public override bool ReleaseCore(Burden burden)
		{
			var genericType = ProxyUtil.GetUnproxiedType(burden.Instance);

			var handler = GetSubHandler(CreationContext.CreateEmpty(), genericType);

			return handler.Release(burden);
		}

		protected IHandler GetSubHandler(CreationContext context, Type genericType)
		{
			IHandler handler;
			if (type2SubHandler.TryGetValue(genericType, out handler))
			{
				return handler;
			}
			lock (type2SubHandler)
			{
				if (type2SubHandler.TryGetValue(genericType, out handler))
				{
					return handler;
				}
				// TODO: we should probably match the requested type to existing services and close them over its generic arguments
				var service = context.RequestedType;
				var newModel = Kernel.ComponentModelFactory.BuildModel(
					ComponentModel.ComponentName, new[] { service }, genericType, ComponentModel.ExtendedProperties);

				newModel.ExtendedProperties[ComponentModel.SkipRegistration] = true;
				CloneParentProperties(newModel);

				// Create the handler and add to type2SubHandler before we add to the kernel.
				// Adding to the kernel could satisfy other dependencies and cause this method
				// to be called again which would result in extra instances being created.
				handler = Kernel.HandlerFactory.Create(newModel);
				type2SubHandler[genericType] = handler;

				Kernel.AddCustomComponent(newModel);

				return handler;
			}
		}

		protected override object Resolve(CreationContext context, bool instanceRequired)
		{
			var implType = GetClosedImplementationType(context, instanceRequired);
			if (implType == null)
			{
				Debug.Assert(instanceRequired == false, "instanceRequired == false");
				return null;
			}

			var handler = GetSubHandler(context, implType);
			// so the generic version wouldn't be considered as well
			using (context.EnterResolutionContext(this, false, false))
			{
				return handler.Resolve(context);
			}
		}

		///<summary>
		///  Clone some of the parent componentmodel properties to the generic subhandler.
		///</summary>
		///<remarks>
		///  The following properties are copied:
		///  <list type = "bullet">
		///    <item>
		///      <description>The <see cref = "LifestyleType" /></description>
		///    </item>
		///    <item>
		///      <description>The <see cref = "ComponentModel.Interceptors" /></description>
		///    </item>
		///  </list>
		///</remarks>
		///<param name = "newModel">the subhandler</param>
		private void CloneParentProperties(ComponentModel newModel)
		{
			// Inherits from LifeStyle's context.
			newModel.LifestyleType = ComponentModel.LifestyleType;

			// Inherit the parent handler interceptors.
			foreach (InterceptorReference interceptor in ComponentModel.Interceptors)
			{
				// we need to check that we are not adding the inteceptor again, if it was added
				// by a facility already
				newModel.Interceptors.AddIfNotInCollection(interceptor);
			}

			if (ComponentModel.HasCustomDependencies)
			{
				var dependencies = newModel.CustomDependencies;
				foreach (DictionaryEntry dependency in ComponentModel.CustomDependencies)
				{
					dependencies.Add(dependency.Key, dependency.Value);
				}
			}
		}

		private Type GetClosedImplementationType(CreationContext context, bool instanceRequired)
		{
			var genericArguments = GetGenericArguments(context);
			try
			{
				// TODO: what if ComponentModel.Implementation is a LateBoundComponent?
				return ComponentModel.Implementation.MakeGenericType(genericArguments);
			}
			catch (ArgumentNullException)
			{
				if (implementationMatchingStrategy == null)
				{
					// NOTE: if we're here something is badly screwed...
					throw;
				}
				throw new HandlerException(
					string.Format("Custom {0} ({1}) didn't select any generic parameters for implementation type of component '{2}'. This usually signifies bug in the {0}.",
					              typeof(IGenericImplementationMatchingStrategy).Name, implementationMatchingStrategy, ComponentModel.Name));
			}
			catch (ArgumentException e)
			{
				// may throw in some cases when impl has generic constraints that service hasn't
				if (instanceRequired == false)
				{
					return null;
				}

				// ok, let's do some investigation now what might have been the cause of the error
				// there can be 3 reasons according to MSDN: http://msdn.microsoft.com/en-us/library/system.type.makegenerictype.aspx

				var arguments = ComponentModel.Implementation.GetGenericArguments();
				string message;
				// 1.
				if (arguments.Length > genericArguments.Length)
				{
					message = string.Format(
						"Requested type {0} has {1} generic parameter(s), whereas component implementation type {2} requires {3}. This means that Windsor does not have enough information to properly create that component for you. This is most likely a bug in your registration code.",
						context.RequestedType,
						context.GenericArguments.Length,
						ComponentModel.Implementation,
						arguments.Length);
					throw new HandlerException(message, e);
				}

				// 2.
				var invalidArguments = genericArguments.Where(a => a.IsPointer || a.IsByRef || a == typeof(void)).Select(t => t.FullName).ToArray();
				if (invalidArguments.Length > 0)
				{
					message = string.Format("The following types provided as generic parameters are not legal: {0}. This is most likely a bug in your code.",
					                        string.Join(", ", invalidArguments));
					throw new HandlerException(message, e);
				}
				// 3. at this point we should be 99% sure we have arguments that don't satisfy generic constraints of out service.
				throw new GenericHandlerTypeMismatchException(genericArguments, ComponentModel, this);
			}
		}

		private Type[] GetGenericArguments(CreationContext context)
		{
			if (implementationMatchingStrategy == null)
			{
				return context.GenericArguments;
			}
			return implementationMatchingStrategy.GetGenericArguments(ComponentModel, context) ?? context.GenericArguments;
		}
	}
}