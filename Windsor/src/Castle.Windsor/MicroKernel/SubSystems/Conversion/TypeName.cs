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
	using System.Linq;

	public class TypeName
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
			get { return String.IsNullOrEmpty(@namespace) == false; }
		}

		private bool IsAssemblyQualified
		{
			get { return assemblyQualifiedName != null; }
		}

		private string Name
		{
			get { return name; }
		}

		public string ExtractAssemblyName()
		{
			if (IsAssemblyQualified == false)
			{
				return null;
			}
			var tokens = assemblyQualifiedName.Split(new[] { ',' }, StringSplitOptions.None);
#if SILVERLIGHT
			var indexOfVersion = -1;
			for (int i = tokens.Length - 1; i >= 0; i--)
			{
				if(tokens[i].TrimStart(' ').StartsWith("Version="))
				{
					indexOfVersion = i;
					break;
				}
			}
#else
			var indexOfVersion = Array.FindLastIndex(tokens, s => s.TrimStart(' ').StartsWith("Version="));
#endif
			if (indexOfVersion <= 0)
			{
				return tokens.Last().Trim();
			}
			return tokens[indexOfVersion - 1].Trim();
		}

		public Type GetType(TypeNameConverter converter)
		{
			if (converter == null)
			{
				throw new ArgumentNullException("converter");
			}
			if (IsAssemblyQualified)
			{
				return Type.GetType(assemblyQualifiedName, false, true);
			}

			Type type;
			if (HasNamespace)
			{
				type = converter.GetTypeByFullName(FullName);
			}
			else
			{
				type = converter.GetTypeByName(Name);
			}

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