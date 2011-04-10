#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.ModelBuilder.Inspectors;

namespace Castle.Services.vNextTransaction
{
	/// <summary>
	/// 	Transaction component inspector that selects the methods
	/// 	available to get intercepted with transactions.
	/// </summary>
	internal class TxComponentInspector : MethodMetaInspector
	{
		private ITxMetaInfoStore _MetaStore;

		public override void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (_MetaStore == null)
				_MetaStore = kernel.Resolve<ITxMetaInfoStore>();

			Contract.Assume(model.Implementation != null);
			Validate(model);
			AddInterceptor(model);
		}

		private void Validate(ComponentModel model)
		{
			Contract.Requires(model.Implementation != null);
			Contract.Ensures(model.Implementation != null);

			Maybe<TxClassMetaInfo> meta;
			List<string> problematicMethods;
			if (model.Service == null
			    || model.Service.IsInterface
			    || !(meta = _MetaStore.GetMetaFromType(model.Implementation)).HasValue
			    || (problematicMethods = (from method in meta.Value.TransactionalMethods
			                              where !method.IsVirtual
			                              select method.Name).ToList()).Count == 0)
				return;

			throw new FacilityException(string.Format("The class {0} wants to use transaction interception, " +
			                                          "however the methods must be marked as virtual in order to do so. Please correct " +
			                                          "the following methods: {1}", model.Implementation.FullName,
			                                          string.Join(", ", problematicMethods.ToArray())));
		}

		private void AddInterceptor(ComponentModel model)
		{
			Contract.Requires(model.Implementation != null);
			var meta = _MetaStore.GetMetaFromType(model.Implementation);

			if (!meta.HasValue)
				return;

			model.Dependencies.Add(new DependencyModel(DependencyType.Service, null, typeof (TxInterceptor), false));
			model.Interceptors.AddFirst(new InterceptorReference(typeof (TxInterceptor)));
		}

		[Pure]
		protected override string ObtainNodeName()
		{
			return "transaction-interceptor";
		}
	}
}