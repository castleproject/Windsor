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

namespace Castle.Core.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Text;

	public abstract class ReflectionUtil
	{
		public static TBase CreateInstance<TBase>(Type subtypeofTBase, params object[] ctorArgs)
		{
			EnsureIsAssignable<TBase>(subtypeofTBase);

			return Instantiate<TBase>(subtypeofTBase, ctorArgs ?? new object[0]);
		}

		public static Assembly GetAssemblyNamed(string assemblyName)
		{
			Debug.Assert(string.IsNullOrEmpty(assemblyName) == false);

			try
			{
				Assembly assembly;
				if (IsAssemblyFile(assemblyName))
				{
#if (SILVERLIGHT)
				assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyName));
#else
					if (Path.GetDirectoryName(assemblyName) == AppDomain.CurrentDomain.BaseDirectory)
					{
						assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyName));
					}
					else
					{
						assembly = Assembly.LoadFile(assemblyName);
					}
#endif
				}
				else
				{
					assembly = Assembly.Load(assemblyName);
				}
				return assembly;
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Could not load assembly {0}", assemblyName), e);
			}
		}

		public static bool IsAssemblyFile(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			string extension;
			try
			{
				extension = Path.GetExtension(filePath);
			}
			catch (ArgumentException)
			{
				// path contains invalid characters...
				return false;
			}
			return IsDll(extension) || IsExe(extension);
		}

		private static void EnsureIsAssignable<TBase>(Type subtypeofTBase)
		{
			if (typeof(TBase).IsAssignableFrom(subtypeofTBase))
			{
				return;
			}

			string message;
			if (typeof(TBase).IsInterface)
			{
				message = String.Format("Type {0} does not implement the interface {1}.", subtypeofTBase.FullName,
				                        typeof(TBase).FullName);
			}
			else
			{
				message = String.Format("Type {0} does not inherit from {1}.", subtypeofTBase.FullName, typeof(TBase).FullName);
			}
			throw new InvalidCastException(message);
		}

		private static TBase Instantiate<TBase>(Type subtypeofTBase, object[] ctorArgs)
		{
			try
			{
				return (TBase)Activator.CreateInstance(subtypeofTBase, ctorArgs);
			}
			catch (MissingMethodException ex)
			{
				string message;
				if (ctorArgs.Length == 0)
				{
					message = String.Format("Type {0} does not have a public default constructor and could not be instantiated.",
					                        subtypeofTBase.FullName);
				}
				else
				{
					var messageBuilder = new StringBuilder();
					messageBuilder.AppendLine(
						String.Format("Type {0} does not have a public constructor matching arguments of the following types:",
						              subtypeofTBase.FullName));
					foreach (var type in Type.GetTypeArray(ctorArgs))
					{
						messageBuilder.AppendLine(type.FullName);
					}
					message = messageBuilder.ToString();
				}
				throw new ArgumentException(message, ex);
			}
			catch (Exception ex)
			{
				var message = String.Format("Could not instantiate {0}.", subtypeofTBase.FullName);
				throw new Exception(message, ex);
			}
		}

		private static bool IsDll(string extension)
		{
			return ".dll".Equals(extension, StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsExe(string extension)
		{
			return ".exe".Equals(extension, StringComparison.OrdinalIgnoreCase);
		}



		public static IEnumerable<Assembly> GetAssemblies(IAssemblyProvider assemblyProvider)
		{
			return assemblyProvider.GetAssemblies();
		}

		public static Assembly GetAssemblyNamed(string filePath, Predicate<AssemblyName> nameFilter, Predicate<Assembly> assemblyFilter)
		{
			AssemblyName assemblyName;
			try
			{
				assemblyName = AssemblyName.GetAssemblyName(filePath);
			}
			catch (ArgumentException)
			{
				assemblyName = new AssemblyName { CodeBase = filePath };
			}
			if (nameFilter != null)
			{
				foreach (Predicate<AssemblyName> predicate in nameFilter.GetInvocationList())
				{
					if (predicate(assemblyName) == false)
					{
						return null;
					}
				}
			}
			var assembly = Assembly.Load(assemblyName);
			if (assemblyFilter != null)
			{
				foreach (Predicate<Assembly> predicate in assemblyFilter.GetInvocationList())
				{
					if (predicate(assembly) == false)
					{
						return null;
					}
				}
			}
			return assembly;
		}
	}
}