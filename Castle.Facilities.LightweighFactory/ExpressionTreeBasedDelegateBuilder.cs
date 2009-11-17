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
			// TODO: use overload of resolve that takes key
			ElementInit[] initializers;
			ParameterExpression[] parameters;
			GetInitializers(out initializers, out parameters, invoke);
			var arguments = new Expression[]
			{ Expression.ListInit(Expression.New(Tokens.HashtableCtor, new Expression[0]), initializers) };
			MethodCallExpression methodCallExpression = Expression.Call(
				Expression.Field(Expression.Constant(factory, typeof(LightweightFactory)), Tokens.LightweightFactoryKernel),
				Tokens.KernelResolve_IDictionary.MakeGenericMethod(invoke.ReturnType),
				arguments);
			return Expression.Lambda(service, methodCallExpression, parameters).Compile();
		}

		#endregion

		private void GetInitializers(out ElementInit[] initializers, out ParameterExpression[] parameters, MethodInfo invoke)
		{
			ParameterInfo[] parameterInfos = invoke.GetParameters();
			initializers = new ElementInit[parameterInfos.Length];
			parameters = new ParameterExpression[parameterInfos.Length];
			for (int i = 0; i < parameterInfos.Length; i++)
			{
				Expression dictionaryValue;
				ConstantExpression dictionaryKey;
				ParameterExpression parameter;
				BuildInitializer(parameterInfos[i], out dictionaryValue, out dictionaryKey, out parameter);
				parameters[i] = parameter;
				initializers[i] = Expression.ElementInit(Tokens.HashtableAdd, new[] { dictionaryKey, dictionaryValue });
			}
		}

		private void BuildInitializer(ParameterInfo parameterInfo, out Expression dictionaryValue,
		                              out ConstantExpression dictionaryKey, out ParameterExpression parameter)
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