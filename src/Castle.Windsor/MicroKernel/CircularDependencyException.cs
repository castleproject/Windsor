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
#if FEATURE_SERIALIZATION
	using System.Runtime.Serialization;
#endif

	using Castle.Core;

    /// <summary>
    ///   Exception throw when a circular dependency is detected
    /// </summary>
#if FEATURE_SERIALIZATION
	[Serializable]
#endif
    public class CircularDependencyException : ComponentResolutionException
	{
		/// <summary>
		///   Initializes a new instance of the
		///   <see cref="CircularDependencyException" />
		///   class.
		/// </summary>
		/// <param name="message">The message.</param>
		public CircularDependencyException(string message) : base(message)
		{
		}

		/// <summary>
		///   Initializes a new instance of the
		///   <see cref="CircularDependencyException" />
		///   class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public CircularDependencyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		///   Initializes a new instance of the
		///   <see cref="CircularDependencyException" />
		///   class.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="component"></param>
		public CircularDependencyException(string message, ComponentModel component)
			: base(message, component)
		{
		}

#if FEATURE_SERIALIZATION
		/// <summary>
		///   Initializes a new instance of the
		///   <see cref="CircularDependencyException" />
		///   class.
		/// </summary>
		/// <param name="info">The
		///   <see cref="T:System.Runtime.Serialization.SerializationInfo" />
		///   that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The
		///   <see cref="T:System.Runtime.Serialization.StreamingContext" />
		///   that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The
		///   <paramref name="info" />
		///   parameter is
		///   <see langword="null" />
		///   .</exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is
		///   <see langword="null" />
		///   or
		///   <see cref="P:System.Exception.HResult" />
		///   is zero (0).</exception>
		protected CircularDependencyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
    }
}