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
	using System.ComponentModel;

	using Castle.Core;

	/// <summary>
	///   Summary description for SupportInitializeConcern.
	/// </summary>
	[Serializable]
	public class SupportInitializeConcern : ICommissionConcern
	{
		private static readonly SupportInitializeConcern instance = new SupportInitializeConcern();

		protected SupportInitializeConcern()
		{
		}

		public void Apply(ComponentModel model, object component)
		{
			var supportInitialize = component as ISupportInitialize;
			if (supportInitialize == null)
			{
				return;
			}
			supportInitialize.BeginInit();
			supportInitialize.EndInit();
		}

		public static SupportInitializeConcern Instance
		{
			get { return instance; }
		}
	}
}