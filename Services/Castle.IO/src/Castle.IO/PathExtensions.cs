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

namespace Castle.IO
{
	using System;
	using System.Diagnostics.Contracts;

	public static class PathExtensions
	{
		public static bool AllowedToAccess(this Path chjailedRootPath, string otherDirectory)
		{
			var other = new Path(otherDirectory);
			// if the given non-root is empty, we are looking at a relative path
			if (String.IsNullOrEmpty(other.Info.Root)) 
				return true;

			// they must be on the same drive.
			if (!String.IsNullOrEmpty(chjailedRootPath.Info.DriveLetter) && other.Info.DriveLetter != chjailedRootPath.Info.DriveLetter)
				return false;

			// we do not allow access to directories outside of the specified directory.
			return other.Info.IsParentOf(chjailedRootPath.Info);
		}
	}
}