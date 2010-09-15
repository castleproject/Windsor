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

namespace Castle.Windsor.Experimental.Debugging
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;

	public class HandlerDebuggerProxy
	{
		private readonly DefaultHandler handler;

		public HandlerDebuggerProxy(DefaultHandler handler)
		{
			this.handler = handler;
		}

		public Type Implementation
		{
			get { return handler.ComponentModel.Implementation; }
		}

		public Type Service
		{
			get { return handler.Service; }
		}

		public string Status
		{
			get
			{
				if (handler.CurrentState == HandlerState.Valid)
				{
					return "All required dependencies can be resolved.";
				}
				return "Some dependencies of this component could not be statically resolved." +
				       handler.ObtainDependencyDetails(new List<object>());
			}
		}

		public object Lifestyle
		{
			get
			{
				var lifestyle = handler.ComponentModel.LifestyleType;
				if (lifestyle == LifestyleType.Custom)
				{
					return "Custom: " + handler.ComponentModel.CustomLifestyle.Name;
				}
				if(lifestyle == LifestyleType.Undefined)
				{
					return string.Format("{0} (default lifestyle {1} will be used)", lifestyle, LifestyleType.Singleton);
				}
				return lifestyle;
			}
		}
	}
}