#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace Castle.Services.Transaction
{
	/// <summary>
	/// The supported transaction mode for the components.
	/// </summary>
	public enum TransactionMode
	{
		/// <summary>
		/// 
		/// </summary>
		Unspecified,

		/// <summary>
		/// transaction context will be created 
		/// managing internally a connection, no 
		/// transaction is opened though
		/// </summary>
		NotSupported,

		/// <summary>
		/// transaction context will be created if not present 
		/// </summary>
		Requires,

		/// <summary>
		/// a new transaction context will be created 
		/// </summary>
		RequiresNew,

		/// <summary>
		/// An existing appropriate transaction context 
		/// will be joined if present, but if if there is no current
		/// transaction on the thread, no transaction will be created.
		/// </summary>
		Supported
	}
}