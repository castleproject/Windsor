// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Linq.Expressions;
	using System.Reflection;

	using Castle.MicroKernel;

	public class ExpressionTreeBasedDelegateBuilder : IDelegateBuilder
	{
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

		private ElementInit[] GetInitializers(MethodInfo invoke, out ParameterExpression[] parameters)
		{
			ParameterInfo[] parameterInfos = invoke.GetParameters();
			var initializers = new ElementInit[parameterInfos.Length];
			parameters = new ParameterExpression[parameterInfos.Length];
			for (var i = 0; i < parameterInfos.Length; i++)
			{
				Expression dictionaryValue;
				Expression dictionaryKey;
				ParameterExpression parameter;

				BuildInitializer(out dictionaryValue, out dictionaryKey, out parameter, parameterInfos[i]);
				parameters[i] = parameter;
				initializers[i] = Expression.ElementInit(Tokens.DictionaryAdd, new[] { dictionaryKey, dictionaryValue });
			}
			return initializers;
		}

		private void BuildInitializer(out Expression dictionaryValue, out Expression dictionaryKey, out ParameterExpression parameter, ParameterInfo parameterInfo)
		{
			if (parameterInfo.IsOut)
			{
				throw new InvalidOperationException("out parameters are not supported.");
			}
			parameter = Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name);
			if (parameterInfo.ParameterType.IsValueType)
			{
				dictionaryKey = Expression.New(Tokens.FactoryParameterCtor,
												 Expression.Constant(parameterInfo.ParameterType),
												 Expression.Constant(parameterInfo.Name),
												 Expression.Constant(parameterInfo.Position),
												 Expression.Convert(parameter, typeof(object)));
				dictionaryValue = Expression.Convert(parameter, typeof(object));
			}
			else
			{
				dictionaryKey = Expression.New(Tokens.FactoryParameterCtor,
												 Expression.Constant(parameterInfo.ParameterType),
												 Expression.Constant(parameterInfo.Name),
												 Expression.Constant(parameterInfo.Position),
												 Expression.Convert(parameter, typeof(object)));
				dictionaryValue = parameter;
			}
		}
	}
}