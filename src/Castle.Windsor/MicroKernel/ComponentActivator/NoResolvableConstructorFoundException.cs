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

namespace Castle.MicroKernel.ComponentActivator
{
	using System;
	using System.Runtime.Serialization;

	using Castle.Core;

	/// <summary>
	///   Exception thrown when component has no resolvable constructor that can be used to create an instance.
	/// </summary>
	[Serializable]
	public class NoResolvableConstructorFoundException : ComponentActivatorException
	{
		private readonly Type type;

		public NoResolvableConstructorFoundException(Type type, ComponentModel componentModel)
			: base(
				string.Format("Could not find resolvable constructor for {0}. Make sure all required dependencies are provided.",
				              type.FullName), componentModel)
		{
			this.type = type;
		}

		public NoResolvableConstructorFoundException(string message, Exception innerException, ComponentModel componentModel)
			: base(message, innerException, componentModel)
		{
		}

#if (!SILVERLIGHT)
		public NoResolvableConstructorFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public Type Type
		{
			get { return type; }
		}
	}
}