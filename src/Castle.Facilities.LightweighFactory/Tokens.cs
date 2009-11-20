namespace Castle.Facilities.LightweighFactory
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;

	using Castle.MicroKernel;

	internal static class Tokens
	{
		// use ListDictionary because it preserves the order
		internal static readonly MethodInfo DictionaryAdd =
			typeof(ListDictionary).GetMethod("Add");
		internal static readonly ConstructorInfo DictionaryCtor =
			typeof(ListDictionary).GetConstructor(Type.EmptyTypes);

		internal static readonly MethodInfo KernelResolve_IDictionary =
			typeof(IKernel).GetMethod("Resolve", BindingFlags.Instance | BindingFlags.Public, new OpenGenericBinder(),
			                          new[] { typeof(string), typeof(IDictionary) }, new ParameterModifier[] { });

		internal static readonly FieldInfo LightweightFactoryKernel =
			typeof(LightweightFactory).GetField("kernel",BindingFlags.Instance |BindingFlags.NonPublic);

		public static readonly ConstructorInfo LightweightResolutionContextCtor =
			typeof(LightweightResolutionContext).GetConstructor(Type.EmptyTypes);

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
				// this is just a single purpoes implementation so we may get away with it being so ugly
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