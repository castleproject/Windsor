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

namespace Castle.IO.FileSystems.Local.Win32.Interop
{
	/// <summary>
	/// 	The sharing mode of an object, which can be read, write, both, delete, all of these, or none (refer to the following table).
	/// 	If this parameter is zero and CreateFileTransacted succeeds, the object cannot be shared and cannot be opened again until the handle is closed. For more information, see the Remarks section of this topic.
	/// 	You cannot request a sharing mode that conflicts with the access mode that is specified in an open request that has an open handle, because that would result in the following sharing violation: ERROR_SHARING_VIOLATION. For more information, see Creating and Opening Files.
	/// </summary>
	[Flags, Serializable]
	public enum NativeFileShare : uint
	{
		/// <summary>
		/// 	Disables subsequent open operations on an object to request any type of access to that object.
		/// </summary>
		None = 0x00,

		/// <summary>
		/// 	Enables subsequent open operations on an object to request read access.
		/// 	Otherwise, other processes cannot open the object if they request read access.
		/// 	If this flag is not specified, but the object has been opened for read access, the function fails.
		/// </summary>
		Read = 0x01,

		/// <summary>
		/// 	Enables subsequent open operations on an object to request write access.
		/// 	Otherwise, other processes cannot open the object if they request write access.
		/// 	If this flag is not specified, but the object has been opened for write access or has a file mapping with write access, the function fails.
		/// </summary>
		Write = 0x02,

		/// <summary>
		/// 	Enables subsequent open operations on an object to request delete access.
		/// 	Otherwise, other processes cannot open the object if they request delete access.
		/// 	If this flag is not specified, but the object has been opened for delete access, the function fails.
		/// </summary>
		Delete = 0x04
	}
}