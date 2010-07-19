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
	using System.Reflection;
	using System.Linq;
	using System.Text;
#if DOTNET35 || SL3
	using System.Reflection.Emit; // needed for .NET 3.5 and SL 3
	using System.Security;
#endif
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

		private readonly ICollection<Assembly> assemblies = 
#if SL3
			new List<Assembly>();
#else
			new HashSet<Assembly>();
#endif

		private readonly IDictionary<string, MultiType> fullName2Type =
			new Dictionary<string, MultiType>(StringComparer.OrdinalIgnoreCase);

		private readonly IDictionary<string, MultiType> justName2Type =
			new Dictionary<string, MultiType>(StringComparer.OrdinalIgnoreCase);

		private readonly ITypeNameParser parser;

#if DOTNET35 || SL3
		private static readonly Type AssemblyBuilderDotNet4 = Type.GetType("System.Reflection.Emit.InternalAssemblyBuilder",
		                                                                   false, true);
#endif

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
				var loadedAssemblies = GetLoadedAssemblies();
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

		private Assembly[] GetLoadedAssemblies()
		{
#if SILVERLIGHT
			var list =
				System.Windows.Deployment.Current.Parts.Select(
					ap => System.Windows.Application.GetResourceStream(new Uri(ap.Source, UriKind.Relative))).Select(
						stream => new System.Windows.AssemblyPart().Load(stream.Stream)).ToArray();
			return list;
#else
			return AppDomain.CurrentDomain.GetAssemblies();
#endif
		}

		protected virtual bool ShouldSkipAssembly(Assembly assembly)
		{
			return assembly == mscorlib || assembly.FullName.StartsWith("System");
		}

		private void Scan(Assembly assembly)
		{
			// won't work for dynamic assemblies
#if DOTNET35 || SL3
			if (assembly is AssemblyBuilder || (AssemblyBuilderDotNet4 != null && assembly.GetType().Equals(AssemblyBuilderDotNet4)))
#else
			if (assembly.IsDynamic)
#endif
			{
				return;
			}
			try
			{
				var exportedTypes = assembly.GetExportedTypes();
				foreach (var type in exportedTypes)
				{
					Insert(fullName2Type, type.FullName, type);
					Insert(justName2Type, type.Name, type);
				}
			}
			catch (NotSupportedException)
			{
				// uncaught dynamic assembly?
				// this seems to happen when .NET 3.5 build runs on .NET 4...
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