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

namespace Castle.MicroKernel
{
	using System;

	/// <summary>
	///   Holds the keys used by Kernel to register/request 
	///   a subsystem.
	/// </summary>
	public abstract class SubSystemConstants
	{
		/// <summary>
		///   Key used for the configuration store subsystem
		/// </summary>
		public static readonly String ConfigurationStoreKey = "config.store";

		/// <summary>
		///   Key used for the conversion manager
		/// </summary>
		public static readonly String ConversionManagerKey = "conversion.mng";

		/// <summary>
		///   Key used for the diagnostics subsystem
		/// </summary>
		public static readonly String DiagnosticsKey = "Castle.DiagnosticsSubSystem";

		/// <summary>
		///   Key used for the naming subsystem
		/// </summary>
		public static readonly String NamingKey = "naming.sub.key";

		/// <summary>
		///   Key used for the resource subsystem
		/// </summary>
		public static readonly String ResourceKey = "resource.sub.key";
	}
}