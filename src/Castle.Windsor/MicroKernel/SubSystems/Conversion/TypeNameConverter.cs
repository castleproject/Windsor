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
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

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

		private readonly ITypeNameParser parser;

		public TypeNameConverter(ITypeNameParser parser)
		{
			if (parser == null)
			{
				throw new ArgumentNullException("parser");
			}

			this.parser = parser;
		}

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

				Debug.Assert((type is MultiType) == false);
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

			var forceLoad = false;
			InitializeAppDomainAssemblies(forceLoad);
			type = GetType(typeName);
			if (type != null)
			{
				return type;
			}

			forceLoad = true;
			if (InitializeAppDomainAssemblies(forceLoad))
			{
				type = GetType(typeName);
			}
			return type;
		}

		private Type GetType(TypeName typeName)
		{
			return typeName.GetType(this);
		}

		private bool InitializeAppDomainAssemblies(bool forceLoad)
		{
			var anyAssemblyAdded = false;
			if (forceLoad || assemblies.Count == 0)
			{
				var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in loadedAssemblies)
				{
					if (assemblies.Contains(assembly) || ShouldSkipAssembly(assembly))
					{
						continue;
					}
					anyAssemblyAdded = true;
					assemblies.Add(assembly);
					Scan(assembly);
				}
			}
			return anyAssemblyAdded;
		}

		protected virtual bool ShouldSkipAssembly(Assembly assembly)
		{
			return assembly == mscorlib || assembly.FullName.StartsWith("System");
		}

		private void Scan(Assembly assembly)
		{
			// won't work for dynamic assemblies
			if (assembly is AssemblyBuilder)
			{
				return;
			}

			foreach (var type in assembly.GetExportedTypes())
			{
				Insert(fullName2Type, type.FullName, type);
				Insert(justName2Type, type.Name, type);
			}
		}

		private void Insert(IDictionary<string, Type> collection, string key, Type value)
		{
			Type existing;
			if (collection.TryGetValue(key, out existing) == false)
			{
				collection[key] = value;
				return;
			}
			var multiType = existing as MultiType;
			if (multiType != null)
			{
				multiType.AddInnerType(value);
				return;
			}
			collection[key] = new MultiType().AddInnerType(existing).AddInnerType(value);
		}

		private TypeName ParseName(string name)
		{
			return parser.Parse(name);
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			return PerformConversion(configuration.Value, targetType);
		}

		public Type GetTypeByFullName(string fullName)
		{
			Type type;
			if (fullName2Type.TryGetValue(fullName, out type))
			{
				EnsureUnique(type, fullName, "assembly qualified name");
			}
			return type;
		}

		public Type GetTypeByName(string justName)
		{
			Type type;
			if (justName2Type.TryGetValue(justName, out type))
			{
				EnsureUnique(type, justName, "full name, or assembly qualified name");
			}
			return type;
		}

		private void EnsureUnique(Type type, string value, string missingInformation)
		{
			if (type is MultiType)
			{
				var multi = type as MultiType;
				var message = new StringBuilder(string.Format("Could not uniquely identify type for '{0}'. ", value));
				message.AppendLine("The following types were matched:");
				foreach (var matchedType in multi)
				{
					message.AppendLine(matchedType.AssemblyQualifiedName);
				}
				message.AppendFormat("Provide more information ({0}) to uniquely identify the type.", missingInformation);
				throw new ConverterException(message.ToString());
			}
		}

		private class MultiType : TypeDelegator, IEnumerable<Type>
		{
			private readonly LinkedList<Type> inner = new LinkedList<Type>();

			public MultiType AddInnerType(Type type)
			{
				inner.AddLast(type);
				return this;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)inner).GetEnumerator();
			}

			public IEnumerator<Type> GetEnumerator()
			{
				return inner.GetEnumerator();
			}
		}
	}
}