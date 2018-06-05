// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration.Internal
{
	using System;
	using System.Reflection;

	/// <summary>
	///   TODO: consider moving this to Castle.Core' Exception helper
	/// </summary>
	public static class ExceptionHelper
	{
		private static readonly MethodInfo PreserveStackTraceMethod = typeof (Exception).GetMethod("InternalPreserveStackTrace",
		                                                                                     BindingFlags.Instance |
		                                                                                     BindingFlags.NonPublic);


		public static Exception PreserveStackTrace(Exception exception)
		{
			if (PreserveStackTraceMethod != null && exception != null)
			{
				PreserveStackTraceMethod.Invoke(exception, null);
			}
			return exception;
		}
	}
}