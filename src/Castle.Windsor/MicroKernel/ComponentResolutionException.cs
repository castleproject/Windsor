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
	using System.Runtime.Serialization;

	using Castle.Core;
	using Castle.Core.Internal;

	/// <summary>
	///   Exception thrown when resolution process for a component was unsuccessful at some point for whatever reason.
	/// </summary>
	[Serializable]
	public class ComponentResolutionException : Exception
	{
		public ComponentModel Component { get; private set; }

		public ComponentResolutionException(string message)
			: base(message)
		{
			ExceptionHelper.SetUp(this);
		}

		public ComponentResolutionException(string message, Exception innerException)
			: base(message, innerException)
		{
			ExceptionHelper.SetUp(this);
		}

		public ComponentResolutionException(string message, ComponentModel component)
			: base(message)
		{
			ExceptionHelper.SetUp(this);
			Component = component;
		}

		public ComponentResolutionException(string message, Exception innerException, ComponentModel component)
			: base(message, innerException)
		{
			ExceptionHelper.SetUp(this);
			Component = component;
		}

		public ComponentResolutionException(ComponentModel component)
		{
			ExceptionHelper.SetUp(this);
			Component = component;
		}

#if (!SILVERLIGHT)
		public ComponentResolutionException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			ExceptionHelper.SetUp(this);
		}
#endif
	}
}