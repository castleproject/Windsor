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
	/// Summary description for InitializationConcern.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class InitializationConcern : ICommissionConcern
	{
		private static readonly InitializationConcern instance = new InitializationConcern();

		public static InitializationConcern Instance
		{
			get { return instance; }
		}

		protected InitializationConcern()
		{
		}

		public void Apply(ComponentModel model, object component)
		{
			var initable = component as IInitializable;
			if (initable != null)
			{
				initable.Initialize();
			}
		}
	}
}