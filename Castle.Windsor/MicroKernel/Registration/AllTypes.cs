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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;

	using Castle.Core.Internal;

	/// <summary>
	///   Describes a set of components to register in the kernel.
	/// </summary>
	public static class AllTypes
	{
		/// <summary>
		///   Prepares to register types from a list of types.
		/// </summary>
		/// <param name = "types">The list of types.</param>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		public static FromTypesDescriptor From(IEnumerable<Type> types)
		{
			return new FromTypesDescriptor(types);
		}

		/// <summary>
		///   Prepares to register types from a list of types.
		/// </summary>
		/// <param name = "types">The list of types.</param>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		public static FromTypesDescriptor From(params Type[] types)
		{
			return new FromTypesDescriptor(types);
		}

		/// <summary>
		///   Prepares to register types from an assembly.
		/// </summary>
		/// <param name = "assembly">The assembly.</param>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		public static FromAssemblyDescriptor FromAssembly(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			return new FromAssemblyDescriptor(assembly);
		}

		/// <summary>
		///   Prepares to register types from an assembly containing the type.
		/// </summary>
		/// <param name = "type">The type belonging to the assembly.</param>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		public static FromAssemblyDescriptor FromAssemblyContaining(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return new FromAssemblyDescriptor(type.Assembly);
		}

		/// <summary>
		///   Prepares to register types from an assembly containing the type.
		/// </summary>
		/// <typeparam name = "T">The type belonging to the assembly.</typeparam>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		public static FromAssemblyDescriptor FromAssemblyContaining<T>()
		{
			return FromAssemblyContaining(typeof(T));
		}

		/// <summary>
		///   Prepares to register types from assemblies found in a given directory that meet additional optional restrictions.
		/// </summary>
		/// <param name = "filter"></param>
		/// <returns></returns>
		public static FromAssemblyDescriptor FromAssemblyInDirectory(AssemblyFilter filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}
			var assemblies = ReflectionUtil.GetAssemblies(filter);
			return new FromAssemblyDescriptor(assemblies);
		}

		/// <summary>
		///   Prepares to register types from an assembly.
		/// </summary>
		/// <param name = "assemblyName">The assembly name.</param>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		public static FromAssemblyDescriptor FromAssemblyNamed(string assemblyName)
		{
			var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
			return FromAssembly(assembly);
		}

		/// <summary>
		///   Prepares to register types from the assembly containing the code invoking this method.
		/// </summary>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		public static FromAssemblyDescriptor FromThisAssembly()
		{
			return FromAssembly(Assembly.GetCallingAssembly());
		}

		/// <summary>
		///   Describes all the types based on <c>basedOn</c>.
		/// </summary>
		/// <param name = "basedOn">The base type.</param>
		/// <returns></returns>
		[Obsolete("Use AllTypes.FromAssembly...BasedOn(basedOn) instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static AllTypesOf Of(Type basedOn)
		{
			return new AllTypesOf(basedOn);
		}

		/// <summary>
		///   Describes all the types based on type T.
		/// </summary>
		/// <typeparam name = "T">The base type.</typeparam>
		/// <returns></returns>
		[Obsolete("Use AllTypes.FromAssembly...BasedOn<T>() instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static AllTypesOf Of<T>()
		{
			return new AllTypesOf(typeof(T));
		}

		/// <summary>
		///   Describes any types that are supplied.
		/// </summary>
		/// <returns></returns>
		[Obsolete("Use AllTypes.FromAssembly...Pick() instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static AllTypesOf Pick()
		{
			return Of<object>();
		}

		/// <summary>
		///   Prepares to register types from a list of types.
		/// </summary>
		/// <param name = "types">The list of types.</param>
		/// <returns>The corresponding <see cref = "FromDescriptor" /></returns>
		[Obsolete("Use From(types) instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static FromTypesDescriptor Pick(IEnumerable<Type> types)
		{
			return new FromTypesDescriptor(types);
		}
	}
}