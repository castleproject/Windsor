#region license

// Copyright 2011 Castle Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using Castle.MicroKernel.Lifestyle;

namespace Castle.Services.vNextTransaction.NHibernate
{
	/// <summary>
	/// 	Hybrid lifestyle manager, 
	/// 	the main lifestyle is <see cref = "PerWebRequestLifestyleManager" />,
	/// 	the secondary lifestyle is <see cref = "TransientLifestyleManager" />
	/// </summary>
	public class HybridPerWebRequestTransientLifestyleManager :
		HybridPerWebRequestLifestyleManager<TransientLifestyleManager>
	{
	}
}