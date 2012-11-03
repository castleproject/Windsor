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

namespace Castle.MicroKernel.Context
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Releasers;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   Used during a component request, passed along to the whole process.
	///   This allow some data to be passed along the process, which is used 
	///   to detected cycled dependency graphs and now it's also being used
	///   to provide arguments to components.
	/// </summary>
	[Serializable]
	public class CreationContext :
#if (!SILVERLIGHT)
		MarshalByRefObject,
#endif
		ISubDependencyResolver
	{
		private readonly ITypeConverter converter;

		private readonly IHandler handler;

		/// <summary>
		///   The list of handlers that are used to resolve
		///   the component.
		///   We track that in order to try to avoid attempts to resolve a service
		///   with itself.
		/// </summary>
		private readonly Stack<IHandler> handlerStack;

		private readonly Type requestedType;

		private readonly Stack<ResolutionContext> resolutionStack;
		private IDictionary additionalArguments;
		private IDictionary extendedProperties;
		private Type[] genericArguments;
		private bool isResolving = true;

		/// <summary>
		///   Initializes a new instance of the <see cref = "CreationContext" /> class.
		/// </summary>
		/// <param name = "requestedType"> The type to extract generic arguments. </param>
		/// <param name = "parentContext"> The parent context. </param>
		/// <param name = "propagateInlineDependencies"> When set to <c>true</c> will clone <paramref name = "parentContext" /> <see cref = "AdditionalArguments" /> . </param>
		public CreationContext(Type requestedType, CreationContext parentContext, bool propagateInlineDependencies)
			: this(parentContext.Handler, parentContext.ReleasePolicy, requestedType, null, null, parentContext)
		{
			if (parentContext == null)
			{
				throw new ArgumentNullException("parentContext");
			}

			if (parentContext.extendedProperties != null)
			{
				extendedProperties = new Arguments(parentContext.extendedProperties);
			}

			if (propagateInlineDependencies && parentContext.HasAdditionalArguments)
			{
				additionalArguments = new Arguments(parentContext.additionalArguments);
			}
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "CreationContext" /> class.
		/// </summary>
		/// <param name = "handler"> The handler. </param>
		/// <param name = "releasePolicy"> The release policy. </param>
		/// <param name = "requestedType"> The type to extract generic arguments. </param>
		/// <param name = "additionalArguments"> The additional arguments. </param>
		/// <param name = "converter"> The conversion manager. </param>
		/// <param name = "parent"> Parent context </param>
		public CreationContext(IHandler handler, IReleasePolicy releasePolicy, Type requestedType,
		                       IDictionary additionalArguments, ITypeConverter converter,
		                       CreationContext parent)
		{
			this.requestedType = requestedType;
			this.handler = handler;
			ReleasePolicy = releasePolicy;
			this.additionalArguments = EnsureAdditionalArgumentsWriteable(additionalArguments);
			this.converter = converter;

			if (parent != null)
			{
				resolutionStack = parent.resolutionStack;
				handlerStack = parent.handlerStack;
				return;
			}
			handlerStack = new Stack<IHandler>(4);
			resolutionStack = new Stack<ResolutionContext>(4);
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "CreationContext" /> class.
		/// </summary>
		private CreationContext()
		{
#pragma warning disable 612,618
			ReleasePolicy = new NoTrackingReleasePolicy();
#pragma warning restore 612,618
			handlerStack = new Stack<IHandler>(4);
			resolutionStack = new Stack<ResolutionContext>(4);
		}

		public IDictionary AdditionalArguments
		{
			get
			{
				if (additionalArguments == null)
				{
					additionalArguments = new Arguments();
				}
				return additionalArguments;
			}
		}

		public Type[] GenericArguments
		{
			get
			{
				if (genericArguments == null)
				{
					genericArguments = ExtractGenericArguments(requestedType);
				}
				return genericArguments;
			}
		}

		public IHandler Handler
		{
			get { return handler; }
		}

		public bool HasAdditionalArguments
		{
			get { return additionalArguments != null && additionalArguments.Count != 0; }
		}

		public virtual bool IsResolving
		{
			get { return isResolving; }
		}

		public IReleasePolicy ReleasePolicy { get; set; }

		public Type RequestedType
		{
			get { return requestedType; }
		}

		public void AttachExistingBurden(Burden burden)
		{
			ResolutionContext resolutionContext;
			try
			{
				resolutionContext = resolutionStack.Peek();
			}
			catch (InvalidOperationException)
			{
				throw new ComponentActivatorException(
					"Not in a resolution context. 'AttachExistingBurden' method can only be called withing a resoltion scope. (after 'EnterResolutionContext' was called within a handler)",
					null);
			}
			resolutionContext.AttachBurden(burden);
		}

		public void BuildCycleMessageFor(IHandler duplicateHandler, StringBuilder message)
		{
			message.AppendFormat("Component '{0}'", duplicateHandler.ComponentModel.Name);

			foreach (var handlerOnTheStack in handlerStack)
			{
				message.AppendFormat(" resolved as dependency of");
				message.AppendLine();
				message.AppendFormat("\tcomponent '{0}'", handlerOnTheStack.ComponentModel.Name);
			}
			message.AppendLine(" which is the root component being resolved.");
		}

		public Burden CreateBurden(IComponentActivator componentActivator, bool trackedExternally)
		{
			ResolutionContext resolutionContext;
			try
			{
				resolutionContext = resolutionStack.Peek();
			}
			catch (InvalidOperationException)
			{
				throw new ComponentActivatorException(
					"Not in a resolution context. 'CreateBurden' method can only be called withing a resoltion scope. (after 'EnterResolutionContext' was called within a handler)",
					null);
			}

			var activator = componentActivator as IDependencyAwareActivator;
			if (activator != null)
			{
				trackedExternally |= activator.IsManagedExternally(resolutionContext.Handler.ComponentModel);
			}

			return resolutionContext.CreateBurden(trackedExternally);
		}

		public ResolutionContext EnterResolutionContext(IHandler handlerBeingResolved, bool requiresDecommission)
		{
			return EnterResolutionContext(handlerBeingResolved, true, requiresDecommission);
		}

		public ResolutionContext EnterResolutionContext(IHandler handlerBeingResolved, bool trackContext,
		                                                bool requiresDecommission)
		{
			var resolutionContext = new ResolutionContext(this, handlerBeingResolved, requiresDecommission, trackContext);
			handlerStack.Push(handlerBeingResolved);
			if (trackContext)
			{
				resolutionStack.Push(resolutionContext);
			}
			return resolutionContext;
		}

		public object GetContextualProperty(object key)
		{
			if (extendedProperties == null)
			{
				return null;
			}

			var value = extendedProperties[key];
			return value;
		}

		/// <summary>
		///   Method used by handlers to test whether they are being resolved in the context.
		/// </summary>
		/// <param name = "handler"> </param>
		/// <returns> </returns>
		/// <remarks>
		///   This method is provided as part of double dispatch mechanism for use by handlers.
		///   Outside of handlers, call <see cref = "IHandler.IsBeingResolvedInContext" /> instead.
		/// </remarks>
		public bool IsInResolutionContext(IHandler handler)
		{
			return handlerStack.Contains(handler);
		}

		public ResolutionContext SelectScopeRoot(Func<IHandler[], IHandler> scopeRootSelector)
		{
			var scopes = resolutionStack.Select(c => c.Handler).Reverse().ToArray();
			var selected = scopeRootSelector(scopes);
			if (selected != null)
			{
				var resolutionContext = resolutionStack.SingleOrDefault(s => s.Handler == selected);
				if (resolutionContext != null)
				{
					return resolutionContext;
				}
			}
			return null;
		}

		public void SetContextualProperty(object key, object value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (extendedProperties == null)
			{
				extendedProperties = new Arguments();
			}
			extendedProperties[key] = value;
		}

		public virtual bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                               ComponentModel model,
		                               DependencyModel dependency)
		{
			return HasAdditionalArguments && (CanResolveByKey(dependency) || CanResolveByType(dependency));
		}

		public virtual object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                              ComponentModel model,
		                              DependencyModel dependency)
		{
			Debug.Assert(CanResolve(context, contextHandlerResolver, model, dependency),
			             "CanResolve(context, contextHandlerResolver, model, dependency)");
			object result = null;
			if (dependency.DependencyKey != null)
			{
				result = Resolve(dependency, additionalArguments[dependency.DependencyKey]);
			}
			return result ?? Resolve(dependency, additionalArguments[dependency.TargetType]);
		}

		private bool CanConvertParameter(Type type)
		{
			return converter != null && converter.CanHandleType(type);
		}

		private bool CanResolve(DependencyModel dependency, object inlineArgument)
		{
			var type = dependency.TargetItemType;
			if (inlineArgument == null || type == null)
			{
				return false;
			}
			return type.IsInstanceOfType(inlineArgument) || CanConvertParameter(type);
		}

		private bool CanResolveByKey(DependencyModel dependency)
		{
			if (dependency.DependencyKey == null)
			{
				return false;
			}
			Debug.Assert(additionalArguments != null, "additionalArguments != null");
			return CanResolve(dependency, additionalArguments[dependency.DependencyKey]);
		}

		private bool CanResolveByType(DependencyModel dependency)
		{
			var type = dependency.TargetItemType;
			if (type == null)
			{
				return false;
			}
			Debug.Assert(additionalArguments != null, "additionalArguments != null");
			return CanResolve(dependency, additionalArguments[type]);
		}

		private IDictionary EnsureAdditionalArgumentsWriteable(IDictionary dictionary)
		{
			// NOTE: this is actually here mostly to workaround the fact that ReflectionBasedDictionaryAdapter is read only
			// we could make it writeable instead, but I'm not sure that would make sense.
			// NOTE: As noted in IOC-ISSUE-190 that may lead to issues with custom IDictionary implementations
			// We better just ignore not known implementations and if someone uses one, it's their problem to take that into
			// account when dealing with DynamicParameters
			if (dictionary == null)
			{
				return null;
			}

			if (!(dictionary is ReflectionBasedDictionaryAdapter))
			{
				return dictionary;
			}
			return new Arguments(dictionary);
		}

		private void ExitResolutionContext(Burden burden, bool trackContext)
		{
			handlerStack.Pop();

			if (trackContext)
			{
				resolutionStack.Pop();
			}
			if (burden == null)
			{
				return;
			}
			if (burden.Instance == null)
			{
				return;
			}
			if (burden.RequiresPolicyRelease == false)
			{
				return;
			}
			if (resolutionStack.Count != 0)
			{
				var parent = resolutionStack.Peek().Burden;
				if (parent == null)
				{
					return;
				}
				parent.AddChild(burden);
			}
		}

		private object Resolve(DependencyModel dependency, object inlineArgument)
		{
			var targetType = dependency.TargetItemType;
			if (inlineArgument != null)
			{
				if (targetType.IsInstanceOfType(inlineArgument))
				{
					return inlineArgument;
				}
				if (CanConvertParameter(targetType))
				{
					return converter.PerformConversion(inlineArgument.ToString(), targetType);
				}
			}
			return null;
		}

		/// <summary>
		///   Creates a new, empty <see cref = "CreationContext" /> instance.
		/// </summary>
		/// <remarks>
		///   A new CreationContext should be created every time, as the contexts keeps some state related to dependency resolution.
		/// </remarks>
		public static CreationContext CreateEmpty()
		{
			return new CreationContext();
		}

		public static CreationContext ForDependencyInspection(IHandler handler)
		{
			var context = CreateEmpty();
			context.isResolving = false;
			context.EnterResolutionContext(handler, false);
			return context;
		}

		private static Type[] ExtractGenericArguments(Type typeToExtractGenericArguments)
		{
			if (typeToExtractGenericArguments.IsGenericType)
			{
				return typeToExtractGenericArguments.GetGenericArguments();
			}
			return Type.EmptyTypes;
		}

		public class ResolutionContext : IDisposable
		{
			private readonly CreationContext context;
			private readonly IHandler handler;
			private readonly bool requiresDecommission;
			private readonly bool trackContext;
			private Burden burden;
			private IDictionary extendedProperties;

			public ResolutionContext(CreationContext context, IHandler handler, bool requiresDecommission, bool trackContext)
			{
				this.context = context;
				this.requiresDecommission = requiresDecommission;
				this.trackContext = trackContext;
				this.handler = handler;
			}

			public Burden Burden
			{
				get { return burden; }
			}

			public CreationContext Context
			{
				get { return context; }
			}

			public IHandler Handler
			{
				get { return handler; }
			}

			public void AttachBurden(Burden burden)
			{
				this.burden = burden;
			}

			public Burden CreateBurden(bool trackedExternally)
			{
				// NOTE: not sure we should allow crreating burden again, when it was already created...
				// this is currently employed by pooled lifestyle
				burden = new Burden(handler, requiresDecommission, trackedExternally);
				return burden;
			}

			public object GetContextualProperty(object key)
			{
				if (extendedProperties == null)
				{
					return null;
				}

				var value = extendedProperties[key];
				return value;
			}

			public void SetContextualProperty(object key, object value)
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				if (extendedProperties == null)
				{
					extendedProperties = new Arguments();
				}
				extendedProperties[key] = value;
			}

			public void Dispose()
			{
				context.ExitResolutionContext(burden, trackContext);
			}
		}
	}
}