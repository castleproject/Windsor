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

namespace Castle.Facilities.WcfIntegration.Lifestyles
{
	using System;
	using System.ServiceModel;

	/// <summary>
	/// Manages object instances in the context of WCF operation. This means that when a component 
	/// with this lifestyle is requested multiple times during WCF operation, the same instance will be provided.
	/// If no WCF operation is available falls back to the default behavior of transient.
	/// </summary>
	public class PerWcfOperationLifestyle : AbstractWcfLifestyleManager<OperationContext, PerOperationCache>
	{
		private readonly IOperationContextProvider contextProvider;

		public PerWcfOperationLifestyle()
			: this(new OperationContextProvider())
		{
		}

		public PerWcfOperationLifestyle(IOperationContextProvider contextProvider)
		{
			if (contextProvider == null)
			{
				throw new ArgumentNullException("contextProvider");
			}

			this.contextProvider = contextProvider;
		}

		protected override OperationContext GetCacheHolder()
		{
			return contextProvider.Current;
		}
	}
}