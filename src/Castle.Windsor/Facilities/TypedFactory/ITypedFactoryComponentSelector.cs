﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Reflection;

	using Castle.MicroKernel;

	public interface ITypedFactoryComponentSelector
	{
		/// <summary>
		///   Selects one or both of component name and type, for given method 
		///   called on given typed factory type.
		///   When component should be requested by type only,
		///   componentName should be null.
		///   When component should be requested by name only,
		///   componentType should be null.
		/// </summary>
		/// <param name = "method"></param>
		/// <param name = "type"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		Func<IKernelInternal, IReleasePolicy, object> SelectComponent(MethodInfo method, Type type, object[] arguments);
	}
}