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

using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using Castle.Services.vNextTransaction;
using log4net;

namespace Castle.Facilities.NHibernate
{
	/// <summary>
	/// 	Hybrid lifestyle manager, 
	/// 	the main lifestyle is <see cref = "PerWebRequestLifestyleManager" />,
	/// 	the secondary lifestyle is <see cref = "TransientLifestyleManager" />
	/// </summary>
	public class HybridPerTransactionTransientLifestyleManager :
		HybridLifestyleManager<PerTransactionLifestyleManager, TransientLifestyleManager>
	{
		private static readonly ILog _Logger = LogManager.GetLogger(typeof (HybridPerTransactionTransientLifestyleManager));

		public override object Resolve(CreationContext context)
		{
			var activeTransaction = Kernel.Resolve<ITxManager>().CurrentTopTransaction.HasValue;

			_Logger.DebugFormat("resolving through hybrid adapter, active transaction: {0}", activeTransaction);

			return activeTransaction
				? lifestyle1.Resolve(context) 
				: lifestyle2.Resolve(context);
		}
	}
}