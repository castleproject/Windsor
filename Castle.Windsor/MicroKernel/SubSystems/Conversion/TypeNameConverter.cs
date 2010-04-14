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
			var type = Type.GetType(name, false, false);
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
			Type type;
			if (typeName.HasNamespace)
			{
				fullName2Type.TryGetValue(typeName.FullName, out type);
				return type;
			}

			justName2Type.TryGetValue(typeName.Name, out type);
			return type;
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
			return new TypeName(@namespace, typeName);
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			return PerformConversion(configuration.Value, targetType);
		}

		private class TypeName
		{
			private readonly string name;
			private readonly string @namespace;

			public TypeName(string @namespace, string name)
			{
				this.name = name;
				this.@namespace = @namespace;
			}

			public string FullName
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

			public bool HasNamespace
			{
				get { return string.IsNullOrEmpty(@namespace) == false; }
			}

			public string Name
			{
				get { return name; }
			}
		}
	}
}