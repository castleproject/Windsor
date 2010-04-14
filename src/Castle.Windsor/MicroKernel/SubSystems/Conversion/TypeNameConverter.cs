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

namespace Castle.MicroKernel.SubSystems.Conversion
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.Core.Configuration;

	/// <summary>
	/// Convert a type name to a Type instance.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class TypeNameConverter : AbstractTypeConverter
	{
		private static readonly Assembly mscorlib = typeof(object).Assembly;
		private readonly ICollection<Assembly> assemblies = new HashSet<Assembly>();

		private readonly IDictionary<string, Type> fullName2Type =
			new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

		private readonly IDictionary<string, Type> justName2Type =
			new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

		public override bool CanHandleType(Type type)
		{
			return type == typeof(Type);
		}

		public override object PerformConversion(String value, Type targetType)
		{
			try
			{
				var type = GetType(value);

				if (type == null)
				{
					var message = String.Format(
						"Could not convert from '{0}' to {1} - Maybe type could not be found",
						value, targetType.FullName);

					throw new ConverterException(message);
				}

				return type;
			}
			catch (ConverterException)
			{
				throw;
			}
			catch (Exception ex)
			{
				var message = String.Format(
					"Could not convert from '{0}' to {1}.",
					value, targetType.FullName);

				throw new ConverterException(message, ex);
			}
		}

		private Type GetType(string name)
		{
			var type = Type.GetType(name, false, true);
			if (type != null)
			{
				return type;
			}
			var typeName = ParseName(name);
			if (typeName == null)
			{
				return null;
			}

			EnsureAppDomainAssembliesInitialized(false);
			type = GetType(typeName);
			return type;
		}

		private Type GetType(TypeName typeName)
		{
			return typeName.GetType(this);
		}

		private void EnsureAppDomainAssembliesInitialized(bool forceLoad)
		{
			if (forceLoad || assemblies.Count == 0)
			{
				var current = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in current)
				{
					if (assemblies.Contains(assembly) || ShouldSkipAssembly(assembly))
					{
						continue;
					}

					assemblies.Add(assembly);
					Scan(assembly);
				}
			}
		}

		protected virtual bool ShouldSkipAssembly(Assembly assembly)
		{
			return assembly == mscorlib || assembly.FullName.StartsWith("System");
		}

		private void Scan(Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				fullName2Type[type.FullName] = type;
				justName2Type[type.Name] = type;
			}
		}

		private TypeName ParseName(string name)
		{
			//ok first let us make sure we're not dealing with fully qualified type name, because in that case there's nothing we can really do.
			if (name.IndexOf(',') != -1)
			{
				return null;
			}
			var genericIndex = name.IndexOf('`');
			var genericTypes = new TypeName[] { };
			if (genericIndex > -1)
			{
				var start = name.IndexOf("[[", genericIndex);
				if (start != -1)
				{
					int count;
					var countString = name.Substring(genericIndex + 1, start - genericIndex - 1);
					if (int.TryParse(countString, out count) == false)
					{
						return null;
					}
					var genericsString = name.Substring(start + 2, name.Length - start - 4);
					genericTypes = ParseNames(genericsString, count);
					if (genericTypes == null)
					{
						return null;
					}
					name = name.Substring(0, start);
				}
			}
			// at this point we assume we have just the type name, probably prefixed with namespace so let's see which one is it
			var typeStartsHere = name.LastIndexOf('.');
			string typeName;
			string @namespace = null;

			if (typeStartsHere > -1 && typeStartsHere < (name.Length - 1))
			{
				typeName = name.Substring(typeStartsHere + 1);
				@namespace = name.Substring(0, typeStartsHere);
			}
			else
			{
				typeName = name;
			}
			return new TypeName(@namespace, typeName, genericTypes);
		}

		private TypeName[] ParseNames(string substring, int count)
		{
			if (count == 1)
			{
				var name = ParseName(substring);
				if (name == null)
				{
					return new TypeName[0];
				}
				return new[] { name };
			}
			return new TypeName[0];
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			return PerformConversion(configuration.Value, targetType);
		}

		private class TypeName
		{
			private readonly string assemblyQualifiedName;
			private readonly TypeName[] genericTypes;
			private readonly string name;
			private readonly string @namespace;

			public TypeName(string @namespace, string name, TypeName[] genericTypes)
			{
				this.name = name;
				this.genericTypes = genericTypes;
				this.@namespace = @namespace;
			}

			public TypeName(string assemblyQualifiedName)
			{
				this.assemblyQualifiedName = assemblyQualifiedName;
			}

			private bool IsAssemblyQualified
			{
				get
				{
					return assemblyQualifiedName != null;
				}
			}

			private string FullName
			{
				get
				{
					if (HasNamespace)
					{
						return @namespace + "." + name;
					}
					throw new InvalidOperationException("Namespace was not defined.");
				}
			}

			private bool HasGenericParameters
			{
				get { return genericTypes.Length > 0; }
			}

			private bool HasNamespace
			{
				get { return string.IsNullOrEmpty(@namespace) == false; }
			}

			private string Name
			{
				get { return name; }
			}

			public Type GetType(TypeNameConverter converter)
			{
				Type type;
				if(IsAssemblyQualified)
				{
					return Type.GetType(assemblyQualifiedName, false, true);
				}
				if (HasNamespace)
				{
					converter.fullName2Type.TryGetValue(FullName, out type);
					return type;
				}

				converter.justName2Type.TryGetValue(Name, out type);
				if (!HasGenericParameters)
				{
					return type;
				}

				var genericArgs = new Type[genericTypes.Length];
				for (var i = 0; i < genericArgs.Length; i++)
				{
					genericArgs[i] = genericTypes[i].GetType(converter);
				}

				return type.MakeGenericType(genericArgs);
			}
		}
	}
}