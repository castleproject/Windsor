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

namespace Castle.IO
{
	/// <summary>
	/// The typed of path parsed out of the path string.
	/// </summary>
	public enum PathType
	{
		/// <summary>
		/// The path is a Win32 device path.
		/// </summary>
		Device,
		/// <summary>
		/// The path is a server UNC path
		/// </summary>
		Server,

		/// <summary>
		/// The path is one for an IPv4 server.
		/// </summary>
		IPv4,

		/// <summary>
		/// The path is one for an IPv6 server.
		/// </summary>
		IPv6,

		/// <summary>
		/// The path is for a drive (normal case).
		/// </summary>
		Drive, // TODO: what about absolute paths not for a specific drive?

		/// <summary>
		/// The path is a relative path.
		/// </summary>
		Relative
	}
}