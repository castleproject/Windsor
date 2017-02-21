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

namespace Castle.MicroKernel.Handlers
{
	using System;
#if FEATURE_SERIALIZATION
	using System.Runtime.Serialization;
#endif
	using Castle.Core;
	using Castle.Core.Internal;

    /// <summary>
    ///   Summary description for HandlerException.
    /// </summary>
#if FEATURE_SERIALIZATION
	[Serializable]
#endif
    public class HandlerException : Exception
	{
		/// <summary>
		///   Initializes a new instance of the <see cref = "HandlerException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		/// <param name = "name"></param>
		public HandlerException(string message, ComponentName name) : base(message)
		{
			ExceptionHelper.SetUp(this);
			Name = name;
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "HandlerException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		/// <param name = "name"></param>
		/// <param name = "innerException"></param>
		public HandlerException(string message, ComponentName name, Exception innerException)
			: base(message, innerException)
		{
			ExceptionHelper.SetUp(this);
			Name = name;
		}

#if FEATURE_SERIALIZATION
		/// <summary>
		///   Initializes a new instance of the <see cref = "HandlerException" /> class.
		/// </summary>
		/// <param name = "info">The object that holds the serialized object data.</param>
		/// <param name = "context">The contextual information about the source or destination.</param>
		public HandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ExceptionHelper.SetUp(this);
		}
#endif

        public ComponentName Name { get; private set; }
	}
}