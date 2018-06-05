﻿// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Diagnostics
{
	/// <summary>
	/// 	Base inteface for exposing diagnostic information about container, its components and extensions
	/// </summary>
	/// <typeparam name = "T"> </typeparam>
	public interface IDiagnostic<out T>
	{
		/// <summary>
		/// 	Collects specified diagnostic information.
		/// </summary>
		/// <returns> </returns>
		T Inspect();
	}
}