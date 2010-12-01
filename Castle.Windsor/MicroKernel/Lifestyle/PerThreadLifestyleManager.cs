// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

#if (!SILVERLIGHT)
namespace Castle.MicroKernel.Lifestyle
{
	using System;

	using Castle.MicroKernel.Context;

	/// <summary>
	///   Summary description for PerThreadLifestyleManager.
	/// </summary>
	[Serializable]
	public class PerThreadLifestyleManager : AbstractLifestyleManager
	{
		private readonly IInstanceScope scope;

		public PerThreadLifestyleManager(IInstanceScope scope)
		{
			this.scope = scope;
		}

		public override void Dispose()
		{
		}

		public override bool Release(object instance)
		{
			// Do nothing.
			return false;
		}

		public override object Resolve(CreationContext context)
		{
			return scope.GetInstance(context, ComponentActivator);
		}
	}
}
#endif