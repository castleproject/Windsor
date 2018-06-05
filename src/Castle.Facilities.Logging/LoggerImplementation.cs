// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.Logging
{
	using System;

	using Castle.Core.Logging;

	/// <summary>
	///   The supported <see cref = "ILoggerFactory" /> implementations.
	/// </summary>
	[Obsolete("A logger factory implementation type should be provided via LogUsing<T>(), this will be removed in the future.")]
	public enum LoggerImplementation
	{
		Custom,
		Null,
		Console,
#if FEATURE_EVENTLOG
		Diagnostics,
#endif
#if CASTLE_SERVICES_LOGGING
		NLog,
		Log4net,
		ExtendedNLog,
		ExtendedLog4net,
#endif
		Trace
	}
}