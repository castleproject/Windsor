namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	public class ParametersBinder: ISubDependencyResolver
	{
		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			var parameters = GetAllNotUsedFactoryParameters(context.AdditionalParameters);
			var result = MatchByName(dependency, parameters);
			if (result != null)
			{
				return result.ResolveValue();
			}
			result = MatchByType(dependency, parameters);
			if (result != null)
			{
				return result.ResolveValue();
			}

			throw new FacilityException("Can't resolve dependency" + dependency);
		}

		private FactoryParameter MatchByType(DependencyModel dependency, IEnumerable<FactoryParameter> parameters)
		{
			return parameters.FirstOrDefault(
				p => dependency.TargetType.IsAssignableFrom(p.Type));
		}

		private FactoryParameter MatchByName(DependencyModel dependency, IEnumerable<FactoryParameter> parameters)
		{
			return parameters.FirstOrDefault(
				p => p.Name.Equals(dependency.DependencyKey, StringComparison.OrdinalIgnoreCase));
		}

		private IEnumerable<FactoryParameter> GetAllNotUsedFactoryParameters(IDictionary additionalParameters)
		{
			return additionalParameters.Values.Cast<object>()
				.Where(p => p is FactoryParameter)
				.Select(p => p as FactoryParameter)
				.Where(p => p.Used == false)
				.OrderBy(p => p.Position);
		}

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			return context != null &&
			       dependency.DependencyType == DependencyType.Parameter &&
			       context.AdditionalParameters != null &&
			       context.AdditionalParameters.Count > 0 &&
			       context.AdditionalParameters.Values.Cast<object>()
			       	.Any(p => p is FactoryParameter);
		}
	}
}