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
using System.Diagnostics.Contracts;

namespace Castle.Services.vNextTransaction
{
	[ContractClass(typeof (TxMetaInfoStoreContract))]
	internal interface ITxMetaInfoStore
	{
		Maybe<TxClassMetaInfo> GetMetaFromType(Type implementation);
	}

	[ContractClassFor(typeof (ITxMetaInfoStore))]
	internal abstract class TxMetaInfoStoreContract : ITxMetaInfoStore
	{
		Maybe<TxClassMetaInfo> ITxMetaInfoStore.GetMetaFromType(Type implementation)
		{
			Contract.Requires(implementation != null);
			var metaFromType = Contract.Result<Maybe<TxClassMetaInfo>>();
			Contract.Ensures(metaFromType != null,
			                 "the meta-info contract must return a non-null maybe (the maybe wraps null, also, so this should be an easy check)");
			return metaFromType;
		}
	}
}