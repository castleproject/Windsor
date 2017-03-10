// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
	using System.Linq;
	using System.Reflection;
	using System.Text;

	using Castle.Core.Configuration;
	using Castle.Core.Internal;

	/// <summary>
	///   Convert a type name to a Type instance.
	/// </summary>
	[Serializable]
	public class TypeNameConverter : AbstractTypeConverter
	{
		private static readonly Assembly mscorlib = typeof(object).Assembly;

		private readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();

		private readonly IDictionary<string, MultiType> fullName2Type =
			new Dictionary<string, MultiType>(StringComparer.OrdinalIgnoreCase);

		private readonly IDictionary<string, MultiType> justName2Type =
			new Dictionary<string, MultiType>(StringComparer.OrdinalIgnoreCase);

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
				return GetType(value);
			}
			catch (ConverterException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ConverterException(String.Format("Could not convert string '{0}' to a type.", value), ex);
			}
		}

		private Type GetType(string name)
		{
			// try to create type using case sensitive search first.
			var type = Type.GetType(name, false, false);
			if (type != null)
			{
				return type;
			}
			// if case sensitive search did not create the type, try case insensitive.
			type = Type.GetType(name, false, true);
			if (type != null)
			{
				return type;
			}
			var typeName = ParseName(name);
			if (typeName == null)
			{
				throw new ConverterException(String.Format("Could not convert string '{0}' to a type. It doesn't appear to be a valid type name.", name));
			}

			InitializeAppDomainAssemblies(forceLoad: false);
			type = typeName.GetType(this);
			if (type != null)
			{
				return type;
			}
			if (InitializeAppDomainAssemblies(forceLoad: true))
			{
				type = typeName.GetType(this);
			}
			if (type != null)
			{
				return type;
			}
			var assemblyName = typeName.ExtractAssemblyName();
			if (assemblyName != null)
			{
				var namePart = assemblyName + ", Version=";
				var assembly = assemblies.FirstOrDefault(a => a.FullName.StartsWith(namePart, StringComparison.OrdinalIgnoreCase));
				if (assembly != null)
				{
					throw new ConverterException(String.Format(
						"Could not convert string '{0}' to a type. Assembly {1} was matched, but it doesn't contain the type. Make sure that the type name was not mistyped.",
						name, assembly.FullName));
				}
				throw new ConverterException(String.Format(
					"Could not convert string '{0}' to a type. Assembly was not found. Make sure it was deployed and the name was not mistyped.",
					name));
			}
			throw new ConverterException(String.Format(
				"Could not convert string '{0}' to a type. Make sure assembly containing the type has been loaded into the process, or consider specifying assembly qualified name of the type.",
				name));
		}

		private bool InitializeAppDomainAssemblies(bool forceLoad)
		{
			var anyAssemblyAdded = false;
			if (forceLoad || assemblies.Count == 0)
			{
				var loadedAssemblies = ReflectionUtil.GetLoadedAssemblies();
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
			if (assembly.IsDynamic)
			{
				return;
			}
			try
			{
				var exportedTypes = assembly.GetAvailableTypes();
				foreach (var type in exportedTypes)
				{
					Insert(fullName2Type, type.FullName, type);
					Insert(justName2Type, type.Name, type);
				}
			}
			catch (NotSupportedException)
			{
				// This might fail in an ASPNET scenario for Desktop CLR
			}
		}

		private void Insert(IDictionary<string, MultiType> collection, string key, Type value)
		{
			MultiType existing;
			if (collection.TryGetValue(key, out existing) == false)
			{
				collection[key] = new MultiType(value);
				return;
			}
			existing.AddInnerType(value);
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
			return GetUniqueType(fullName, fullName2Type, "assembly qualified name");
		}

		public Type GetTypeByName(string justName)
		{
			return GetUniqueType(justName, justName2Type, "full name, or assembly qualified name");
		}

		private Type GetUniqueType(string name, IDictionary<string, MultiType> map, string description)
		{
			MultiType type;
			if (map.TryGetValue(name, out type))
			{
				EnsureUnique(type, name, description);
				return type.Single();
			}
			return null;
		}

		private void EnsureUnique(MultiType type, string value, string missingInformation)
		{
			if (type.HasOne)
			{
				return;
			}

			var message = new StringBuilder(string.Format("Could not uniquely identify type for '{0}'. ", value));
			message.AppendLine("The following types were matched:");
			foreach (var matchedType in type)
			{
				message.AppendLine(matchedType.AssemblyQualifiedName);
			}
			message.AppendFormat("Provide more information ({0}) to uniquely identify the type.", missingInformation);
			throw new ConverterException(message.ToString());
		}

		private class MultiType : IEnumerable<Type>
		{
			private readonly LinkedList<Type> inner = new LinkedList<Type>();

			public MultiType(Type type)
			{
				inner.AddFirst(type);
			}

			public bool HasOne
			{
				get { return inner.Count == 1; }
			}

			public MultiType AddInnerType(Type type)
			{
				inner.AddLast(type);
				return this;
			}

			public IEnumerator<Type> GetEnumerator()
			{
				return inner.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)inner).GetEnumerator();
			}
		}
	}
}