#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;

namespace Castle.Services.Transaction.Lifestyles
{
	/// <summary>
	/// 	Exception thrown when there's no transaction available when the component is resolved and the component
	///		requires a per-transaction lifestyle.
	/// </summary>
	/// <remarks>
	/// 	Contains a custom property, thus it Implements ISerializable 
	/// 	and the special serialization constructor.
	/// </remarks>
	[Serializable]
	public sealed class MissingTransactionException : Exception
	{
		/// <summary>
		/// 	Initializes a new instance of the <see cref = "MissingTransactionException" /> class.
		/// </summary>
		public MissingTransactionException()
		{
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref = "MissingTransactionException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		public MissingTransactionException(string message) : base(message)
		{
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref = "MissingTransactionException" /> class.
		/// </summary>
		/// <param name = "message">The message.</param>
		/// <param name = "innerException">The inner exception.</param>
		public MissingTransactionException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}