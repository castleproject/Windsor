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

namespace Castle.MicroKernel.Util
{
	using System;

	public abstract class ReferenceExpressionUtil
	{
		public static string BuildReference(string value)
		{
			if (IsReference(value))
			{
				return value;
			}
			return String.Format("${{{0}}}", value);
		}

		public static String ExtractComponentName(String value)
		{
			if (IsReference(value))
			{
				return value.Substring(2, value.Length - 3);
			}

			return null;
		}

		public static bool IsReference(String value)
		{
			return value != null && value.Length > 3 && value.StartsWith("${") && value.EndsWith("}");
		}
	}
}