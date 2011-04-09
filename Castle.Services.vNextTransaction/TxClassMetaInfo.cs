#region license

// // Copyright 2009-2011 Henrik Feldt - http://logibit.se /
// // 
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// // 
// //     http://www.apache.org/licenses/LICENSE-2.0
// // 
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Castle.Services.vNextTransaction
{
	internal sealed class TxClassMetaInfo
	{
		private readonly Dictionary<MethodInfo, TransactionAttribute> _TxMethods;
		private readonly HashSet<MethodInfo> _NormalMethods;

		public TxClassMetaInfo(IEnumerable<Tuple<MethodInfo, TransactionAttribute>> methods)
		{
			Contract.Requires(methods != null);
			Contract.Ensures(_TxMethods != null);
			Contract.Ensures(_NormalMethods != null);

			_TxMethods = new Dictionary<MethodInfo, TransactionAttribute>(methods.Count());
			_NormalMethods = new HashSet<MethodInfo>();

			methods.Where(x => x.Item2 != null).Run(pair => _TxMethods.Add(pair.Item1, pair.Item2));
			methods.Where(x => x.Item2 == null).Run(pair => _NormalMethods.Add(pair.Item1));
		}


		public IEnumerable<MethodInfo> TransactionalMethods
		{
			get
			{
				Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);
				return _TxMethods.Keys;
			}
		}
	}
}