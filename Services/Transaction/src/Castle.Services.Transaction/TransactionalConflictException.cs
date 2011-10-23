#region license

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

#endregion

using System;
using System.Runtime.Serialization;

namespace Castle.Services.Transaction
{
	/// <summary>
	/// Thrown if a non-transacted file API and a transacted file API try and operate on the same inode/file.
	/// </summary>
	[Serializable]
	public sealed class TransactionalConflictException : TransactionException
	{
		public TransactionalConflictException()
		{
		}

		public TransactionalConflictException(string message) : base(message)
		{
		}

		public TransactionalConflictException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public TransactionalConflictException(string message, Uri helperLink) : base(message, helperLink)
		{
		}

		public TransactionalConflictException(string message, Exception innerException, Uri helperLink)
			: base(message, innerException, helperLink)
		{
		}

		private TransactionalConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}