namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;

	public class ParametersBinder : ISubDependencyResolver
	{
		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                      DependencyModel dependency)
		{
			var parameters = GetAllNotUsedFactoryParameters(GetAllFactoryParameters(context));
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

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
		                       DependencyModel dependency)
		{
			if (context == null || dependency.DependencyType != DependencyType.Parameter)
			{
				return false;
			}
			if (context.AdditionalParameters == null)
			{
				return false;
			}
			if (context.AdditionalParameters.Count <= 0)
			{
				return false;
			}
			var factoryParameters = GetAllFactoryParameters(context);
			if (!factoryParameters.Any())
			{
				return false;
			}
			return CanResolve(dependency, GetAllNotUsedFactoryParameters(factoryParameters));
		}

		private FactoryParameter MatchByType(DependencyModel dependency, IEnumerable<FactoryParameter> parameters)
		{
			return parameters.FirstOrDefault(p => dependency.TargetType == p.Type);
		}

		private FactoryParameter MatchByName(DependencyModel dependency, IEnumerable<FactoryParameter> parameters)
		{
			return parameters.FirstOrDefault(
				p => p.Name.Equals(dependency.DependencyKey, StringComparison.OrdinalIgnoreCase));
		}

		private IEnumerable<FactoryParameter> GetAllNotUsedFactoryParameters(IEnumerable<FactoryParameter> parameters)
		{
			return parameters.Where(p => p.Used == false).OrderBy(p => p.Position);
		}

		private IEnumerable<FactoryParameter> GetAllFactoryParameters(CreationContext context)
		{
			return context.AdditionalParameters.Keys.Cast<object>().Where(p => p is FactoryParameter).Cast<FactoryParameter>();
		}

		private bool CanResolve(DependencyModel dependency, IEnumerable<FactoryParameter> parameters)
		{
			var result = MatchByName(dependency, parameters);
			if (result != null)
			{
				return true;
			}

			result = MatchByType(dependency, parameters);
			if (result != null)
			{
				return true;
			}
			return false;
		}
	}
}