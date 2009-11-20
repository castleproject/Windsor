namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Linq.Expressions;
	using System.Reflection;

	using Castle.MicroKernel;

	public class ExpressionTreeBasedDelegateBuilder : IDelegateBuilder
	{
		#region IDelegateBuilder Members

		public Delegate BuildDelegate(IHandler handler, MethodInfo invoke, Type service, LightweightFactory factory)
		{
			ParameterExpression[] parameters;
			var initializers = GetInitializers(invoke, out parameters);
			var arguments = new Expression[]
			{
				Expression.Constant(handler.ComponentModel.Name),
				Expression.ListInit(Expression.New(Tokens.DictionaryCtor, new Expression[0]), initializers)
			};
			var methodCallExpression = Expression.Call(
				Expression.Field(Expression.Constant(factory, typeof(LightweightFactory)), Tokens.LightweightFactoryKernel),
				Tokens.KernelResolve_IDictionary.MakeGenericMethod(invoke.ReturnType),
				arguments);
			return Expression.Lambda(service, methodCallExpression, parameters).Compile();
		}

		#endregion

		private ElementInit[] GetInitializers(MethodInfo invoke, out ParameterExpression[] parameters)
		{
			
			ParameterInfo[] parameterInfos = invoke.GetParameters();
			var initializers = new ElementInit[parameterInfos.Length + 1];
			parameters = new ParameterExpression[parameterInfos.Length];
			for (var i = 0; i < parameterInfos.Length; i++)
			{
				Expression dictionaryValue;
				ConstantExpression dictionaryKey;
				ParameterExpression parameter;

				BuildInitializer(out dictionaryValue, out dictionaryKey, out parameter, parameterInfos[i]);
				parameters[i] = parameter;
				initializers[i] = Expression.ElementInit(Tokens.DictionaryAdd, new[] { dictionaryKey, dictionaryValue });
			}
			initializers[initializers.Length - 1] =
				Expression.ElementInit(Tokens.DictionaryAdd,
				                       new Expression[]
				                       {
				                       	Expression.Constant("lightweight-facility-resolution-context"),
				                       	Expression.New(Tokens.LightweightResolutionContextCtor)
				                       });
			return initializers;
		}

		private void BuildInitializer(out Expression dictionaryValue, out ConstantExpression dictionaryKey, out ParameterExpression parameter, ParameterInfo parameterInfo)
		{
			if (parameterInfo.IsOut)
			{
				throw new InvalidOperationException("out parameters are not supported.");
			}
			parameter = Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name);
			dictionaryKey = Expression.Constant(parameterInfo.Name, typeof(string));
			if (parameterInfo.ParameterType.IsValueType)
			{
				dictionaryValue = Expression.Convert(parameter, typeof(object));
			}
			else
			{
				dictionaryValue = parameter;
			}
		}
	}
}