// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Util
{
	using System;

	// TODO: this type seems to not be used anywhere. Is it safe to delete it, or someone depends on it?
	public static class GenericTypeNameProvider
	{
		public static string AppendGenericTypeName(IHandler handler, Type genericService, string key)
		{
			return key + "-->" + genericService.FullName;
		}

		public static string StripGenericTypeName(string key)
		{
			int genericParamStartIndex = key.IndexOf("-->");
			if (genericParamStartIndex != -1)
				key = key.Substring(0, genericParamStartIndex);
			return key;
		}
	}
}
