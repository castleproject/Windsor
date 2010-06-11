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

	public class ExpressionTreeBasedDelegateGenerator : IDelegateGenerator
	{
		public Delegate BuildDelegate(DelegateInvocation invocation, MethodInfo invoke, Type delegateType)
		{
			ParameterExpression[] parameters;
			Expression[] argumentsAsObjects = GetArrayInitArguments(invoke, out parameters);

			var lambda =
				Expression.Lambda(delegateType,
					Expression.Convert(
						Expression.Call(Expression.Constant(invocation), DelegateInvocation.InvokeToken,
						                new Expression[]
						                {
						                	Expression.NewArrayInit(typeof(object),
						                	                        argumentsAsObjects)
						                }), invoke.ReturnType),
					parameters);
			return lambda.Compile();
		}

		private Expression[] GetArrayInitArguments(MethodInfo invoke, out ParameterExpression[] parameters)
		{
			var parameterInfos = invoke.GetParameters();
			var initializers = new Expression[parameterInfos.Length];
			parameters = new ParameterExpression[parameterInfos.Length];
			for (var i = 0; i < parameterInfos.Length; i++)
			{
				ParameterExpression parameter;
				initializers[i] = GetParameter(out parameter, parameterInfos[i]);
				parameters[i] = parameter;
			}
			return initializers;
		}

		private Expression GetParameter(out ParameterExpression parameter, ParameterInfo parameterInfo)
		{
			if (parameterInfo.IsOut)
			{
				throw new InvalidOperationException("out parameters are not supported.");
			}
			parameter = Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name);
			if (parameterInfo.ParameterType.IsValueType)
			{
				return Expression.Convert(parameter, typeof(object));
			}
			return parameter;
		}
	}
}