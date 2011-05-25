#region license

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

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Castle.Facilities.Transactions;

namespace Castle.Facilities.Transactions
{
	internal sealed class TransactionalClassMetaInfo
	{
		private readonly Dictionary<MethodInfo, TransactionAttribute> _TxMethods;
		private readonly HashSet<MethodInfo> _NormalMethods;

		public TransactionalClassMetaInfo(IEnumerable<Tuple<MethodInfo, TransactionAttribute>> methods)
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
			[Pure]
			get
			{
				Contract.Ensures(Contract.Result<IEnumerable<MethodInfo>>() != null);
				return _TxMethods.Keys;
			}
		}

		/// <summary>
		/// 	Gets the maybe transaction options for the method info, target. If the target
		/// 	has not been associated with a tranaction, the maybe is none.
		/// </summary>
		/// <param name = "target">Method to find the options for.</param>
		/// <returns>A non-null maybe <see cref = "ITransactionOptions" />.</returns>
		[Pure]
		public Maybe<ITransactionOptions> AsTransactional(MethodInfo target)
		{
			Contract.Requires(target != null);
			Contract.Ensures(Contract.Result<Maybe<TransactionAttribute>>() != null);
			return _TxMethods.ContainsKey(target)
			       	? Maybe.Some<ITransactionOptions>(_TxMethods[target])
			       	: Maybe.None<ITransactionOptions>();
		}
	}
}