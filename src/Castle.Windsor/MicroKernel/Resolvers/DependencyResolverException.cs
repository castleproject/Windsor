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

namespace Castle.MicroKernel.Resolvers
{
	using System;
#if FEATURE_SERIALIZATION
	using System.Runtime.Serialization;
#endif
	using Castle.Core.Internal;

#if FEATURE_SERIALIZATION
	[Serializable]
#endif
    public class DependencyResolverException : Exception
	{
		/// <summary>
		///   Initializes a new instance of the <see cref = "DependencyResolverException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		/// <param name = "innerException">The inner exception.</param>
		public DependencyResolverException(string message, Exception innerException) : base(message, innerException)
		{
			ExceptionHelper.SetUp(this);
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "DependencyResolverException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		public DependencyResolverException(string message) : base(message)
		{
			ExceptionHelper.SetUp(this);
		}

#if FEATURE_SERIALIZATION
		/// <summary>
		///   Initializes a new instance of the <see cref = "DependencyResolverException" /> class.
		/// </summary>
		/// <param name = "info">The object that holds the serialized object data.</param>
		/// <param name = "context">The contextual information about the source or destination.</param>
		public DependencyResolverException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ExceptionHelper.SetUp(this);
		}
#endif
    }
}