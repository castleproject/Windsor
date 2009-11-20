namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Collections;
	using System.Diagnostics;

	using Castle.Core;
	using Castle.MicroKernel;

	public class ParametersBinder: ISubDependencyResolver
	{
		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			Debug.Assert(context.AdditionalParameters.Contains(dependency.DependencyKey) == false,
						 "context.AdditionalParameters.Contains(dependency.DependencyKey) == false");
			var resolutionContext = context.AdditionalParameters["lightweight-facility-resolution-context"] as LightweightResolutionContext;
			Debug.Assert(resolutionContext != null, "resolutionContext != null");
			//let's match by type...
			var type = dependency.TargetType;
			var items = GetAllOfType(context.AdditionalParameters, type);
			//...and get first of non-used dependencies
			return resolutionContext.NextNotUsed(items);

		}

		private IEnumerable GetAllOfType(IDictionary additionalParameters, Type type)
		{
			foreach (var item in additionalParameters.Values)
			{
				if (type.IsAssignableFrom(item.GetType()))
					yield return item;
			}

		}

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			return context != null &&
			       context.AdditionalParameters != null &&
			       context.AdditionalParameters.Count > 0 &&
			       dependency.DependencyType == DependencyType.Parameter &&
			       context.AdditionalParameters.Contains("lightweight-facility-resolution-context");
		}
	}
}