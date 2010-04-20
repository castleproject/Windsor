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
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;

	public abstract class ReflectionUtil
	{
		public static Assembly GetAssemblyNamed(string assemblyName)
		{
			Debug.Assert(string.IsNullOrEmpty(assemblyName) == false);
			
			try
			{
				Assembly assembly;
				var extension = Path.GetExtension(assemblyName);
				if (IsDll(extension) || IsExe(extension))
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

		private static bool IsExe(string extension)
		{
			return ".exe".Equals(extension,StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsDll(string extension)
		{
			return ".dll".Equals(extension,StringComparison.OrdinalIgnoreCase);
		}
	}
}