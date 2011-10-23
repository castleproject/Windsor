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

namespace Castle.MicroKernel.SubSystems.Conversion
{
	using System;
	using System.Runtime.Serialization;

	using Castle.Core.Internal;

	[Serializable]
	public class ConverterException : Exception
	{
		/// <summary>
		///   Initializes a new instance of the <see cref = "ConverterException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		public ConverterException(string message) : base(message)
		{
			HelpLink = Constants.ExceptionHelpLink;
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "ConverterException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		/// <param name = "innerException">The inner exception.</param>
		public ConverterException(string message, Exception innerException) : base(message, innerException)
		{
			HelpLink = Constants.ExceptionHelpLink;
		}

#if (!SILVERLIGHT)
		/// <summary>
		///   Initializes a new instance of the <see cref = "ConverterException" /> class.
		/// </summary>
		/// <param name = "info">The object that holds the serialized object data.</param>
		/// <param name = "context">The contextual information about the source or destination.</param>
		public ConverterException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			HelpLink = Constants.ExceptionHelpLink;
		}
#endif
	}
}