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
	using System.Collections;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;

	using Castle.MicroKernel;

	internal static class Tokens
	{
		internal static readonly MethodInfo DictionaryAdd =
			typeof(FactoryArguments).GetMethod("Add");
		internal static readonly ConstructorInfo DictionaryCtor =
			typeof(FactoryArguments).GetConstructor(Type.EmptyTypes);

		internal static readonly MethodInfo KernelResolve_IDictionary =
			typeof(IKernel).GetMethod("Resolve", BindingFlags.Instance | BindingFlags.Public, new OpenGenericBinder(),
			                          new[] { typeof(string), typeof(IDictionary) }, new ParameterModifier[] { });

		internal static readonly FieldInfo LightweightFactoryKernel =
			typeof(LightweightFactory).GetField("kernel",BindingFlags.Instance |BindingFlags.NonPublic);

		internal static readonly ConstructorInfo FactoryParameterCtor = typeof(FactoryParameter).GetConstructor(new[]
		{
			typeof(Type),
			typeof(string),
			typeof(int),
			typeof(object)
		});

		private class OpenGenericBinder : Binder
		{
			public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
			{
				throw new NotImplementedException();
			}

			public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
			{
				throw new NotImplementedException();
			}

			public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
			{
				// this is just a single purpose implementation so we may get away with it being so ugly
				return match.Single(m => m.IsGenericMethodDefinition);
			}

			public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
			{
				throw new NotImplementedException();
			}

			public override object ChangeType(object value, Type type, CultureInfo culture)
			{
				throw new NotImplementedException();
			}

			public override void ReorderArgumentArray(ref object[] args, object state)
			{
				throw new NotImplementedException();
			}
		}
	}
}