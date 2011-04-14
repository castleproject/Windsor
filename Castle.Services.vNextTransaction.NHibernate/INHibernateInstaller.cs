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

using FluentNHibernate.Cfg;
using NHibernate;

namespace Castle.Services.vNextTransaction.NHibernate
{
	/// <summary>
	/// 	Register a bunch of these; one for each database.
	/// </summary>
	public interface INHibernateInstaller
	{
		/// <summary>
		/// 	Is this the default session factory
		/// </summary>
		bool IsDefault { get; }

		/// <summary>
		/// </summary>
		string SessionFactoryKey { get; }

		/// <summary>
		/// Build a fluent configuration.
		/// </summary>
		/// <returns>A non null fluent configuration instance that can
		/// be used to further configure NHibernate</returns>
		FluentConfiguration BuildFluent();

		/// <summary>
		/// Call-back to the installer, when the factory is registered
		/// and correctly set up in Windsor..
		/// </summary>
		/// <param name="factory"></param>
		void Registered(ISessionFactory factory);
	}
}