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
using FluentNHibernate.Cfg;
using NHibernate;

namespace Castle.Services.vNextTransaction.NHibernate
{
	[ContractClassFor(typeof (INHibernateInstaller))]
	internal class INHibernateInstallerContract : INHibernateInstaller
	{
		public bool IsDefault
		{
			get { throw new NotImplementedException(); }
		}

		public string SessionFactoryKey
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null && Contract.Result<string>().Length > 0);
				throw new NotImplementedException();
			}
		}

		public Maybe<IInterceptor> Interceptor
		{
			get
			{
				Contract.Ensures(Contract.Result<IInterceptor>() != null);
				throw new NotImplementedException();
			}
		}

		public FluentConfiguration BuildFluent()
		{
			Contract.Ensures(Contract.Result<FluentConfiguration>() != null);
			throw new NotImplementedException();
		}

		public void Registered(ISessionFactory factory)
		{
			Contract.Requires(factory != null);
			throw new NotImplementedException();
		}
	}
}