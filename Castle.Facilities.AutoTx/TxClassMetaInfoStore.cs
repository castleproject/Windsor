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

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Castle.Services.Transaction;
using Castle.Services.Transaction.Utils;

namespace Castle.Facilities.AutoTx
{
	internal sealed class TxClassMetaInfoStore : ITxMetaInfoStore
	{
		private readonly Func<Type, Maybe<TxClassMetaInfo>> _GetMetaFromType;

		public TxClassMetaInfoStore()
		{
			Contract.Ensures(_GetMetaFromType != null);
			_GetMetaFromType = Fun.Memoize<Type, Maybe<TxClassMetaInfo>>(GetMetaFromTypeInner);
		}

		internal static Maybe<TxClassMetaInfo> GetMetaFromTypeInner(Type implementation)
		{
			Contract.Ensures(Contract.Result<Maybe<TxClassMetaInfo>>() != null);

			var allMethods =
				(from m in implementation.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
				                                     BindingFlags.DeclaredOnly)
				 let attribs = (TransactionAttribute[]) m.GetCustomAttributes(typeof (TransactionAttribute), true)
				 where attribs.Length > 0
				 select Tuple.Create(m, attribs.Length > 0 ? attribs[0] : null)).ToList();

			if (allMethods.Count > 0)
				return Maybe.Some(new TxClassMetaInfo(allMethods));

			return Maybe.None<TxClassMetaInfo>();
		}

		Maybe<TxClassMetaInfo> ITxMetaInfoStore.GetMetaFromType(Type implementation)
		{
			var maybe = _GetMetaFromType(implementation);
			Contract.Assume(maybe != null);
			return maybe;
		}
	}
}