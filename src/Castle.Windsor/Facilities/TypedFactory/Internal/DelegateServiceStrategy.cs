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

namespace Castle.Facilities.TypedFactory.Internal
{
	using System;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Handlers;

	public class DelegateServiceStrategy : IGenericServiceStrategy
	{
		public static readonly DelegateServiceStrategy Instance = new DelegateServiceStrategy();

		private DelegateServiceStrategy()
		{
		}

		public bool Supports(Type service, ComponentModel component)
		{
			var invoke = DelegateFactory.ExtractInvokeMethod(service);
			return invoke != null && invoke.ReturnType.IsPrimitiveTypeOrCollection() == false;
		}
	}
}