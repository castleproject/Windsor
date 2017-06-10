// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

using System;

#if DOTNET45
namespace Castle.Core.Internal
{
	// This should be changed to expose externals similar to how core does it. Rather then copy this down to tests we should 
	// be using the InternalsVisibleTo attribute. Will raise this on the PR in GitHub. Making it public for now to get
	// the project to compile. If you are reviewing this, please flag it.
    internal static class AppContext
    {
        public static string BaseDirectory
        {
            get
            {
				return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
    }
}
#endif