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

namespace Castle.MicroKernel
{
	using System;
	using System.Security;

	[Serializable]
#if (SILVERLIGHT)
	public abstract class AbstractSubSystem : ISubSystem
#else
	public abstract class AbstractSubSystem : MarshalByRefObject, ISubSystem
#endif
	{
		private IKernelInternal kernel;

#if (!SILVERLIGHT)
#if !DOTNET35
		[SecurityCritical]
#endif
		public override object InitializeLifetimeService()
		{
			return null;
		}
#endif

		public virtual void Init(IKernelInternal kernel)
		{
			this.kernel = kernel;
		}

		public virtual void Terminate()
		{
		}

		protected IKernelInternal Kernel
		{
			get { return kernel; }
		}
	}
}