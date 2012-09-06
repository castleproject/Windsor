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
	using System.Collections.Generic;

	using Castle.Core;

	public static class HandlerExtensionsUtil
	{
		public static readonly string ReleaseExtensionsKey = "Castle.ReleaseExtensions";
		public static readonly string ResolveExtensionsKey = "Castle.ResolveExtensions";

		public static ICollection<IReleaseExtension> ReleaseExtensions(this ComponentModel model, bool ensureExists)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}

			var releaseExtensions = model.ExtendedProperties[ReleaseExtensionsKey] as ICollection<IReleaseExtension>;
			if (releaseExtensions == null && ensureExists)
			{
				releaseExtensions = new HashSet<IReleaseExtension>();
				model.ExtendedProperties[ReleaseExtensionsKey] = releaseExtensions;
			}
			return releaseExtensions;
		}

		public static ICollection<IResolveExtension> ResolveExtensions(this ComponentModel model, bool ensureExists)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}

			var resolveExtensions = model.ExtendedProperties[ResolveExtensionsKey] as ICollection<IResolveExtension>;
			if (resolveExtensions == null && ensureExists)
			{
				resolveExtensions = new HashSet<IResolveExtension>();
				model.ExtendedProperties[ResolveExtensionsKey] = resolveExtensions;
			}
			return resolveExtensions;
		}
	}
}