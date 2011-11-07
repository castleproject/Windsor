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

namespace Castle.MicroKernel.LifecycleConcerns
{
	using System;

	using Castle.Core;

	/// <summary>
	/// Summary description for DisposalConcern.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class DisposalConcern : IDecommissionConcern
	{
		private static readonly DisposalConcern instance = new DisposalConcern();

		public static DisposalConcern Instance
		{
			get { return instance; }
		}

		protected DisposalConcern()
		{
		}

		public void Apply(ComponentModel model, object component)
		{
			var disposable = component as IDisposable;
			if (disposable == null)
			{
				return;
			}
			disposable.Dispose();
		}
	}
}