// Copyright 2022 Castle Project - http://www.castleproject.org/
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
	using Castle.Core.Logging;

	public static class BuiltInLoggingFactoryExtensions
	{
		public static LoggingFacility LogUsingNullLogger(this LoggingFacility loggingFacility) => loggingFacility.LogUsing<NullLogFactory>();
		public static LoggingFacility LogUsingConsoleLogger(this LoggingFacility loggingFacility) => loggingFacility.LogUsing<ConsoleFactory>();

#if NET6_0_OR_GREATER
		[System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
		public static LoggingFacility LogUsingDiagnosticsLogger(this LoggingFacility loggingFacility) => loggingFacility.LogUsing<DiagnosticsLoggerFactory>();
		public static LoggingFacility LogUsingTraceLogger(this LoggingFacility loggingFacility) => loggingFacility.LogUsing<TraceLoggerFactory>();
	}
}
