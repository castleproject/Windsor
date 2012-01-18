#region License

//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

#endregion

namespace Castle.Facilities.NHibernateIntegration.SessionStores
{
	using System;
	using System.Collections;
	using System.Runtime.Remoting.Messaging;

	/// <summary>
	/// Provides an implementation of <see cref="ISessionStore"/>
	/// which relies on <c>CallContext</c>
	/// </summary>
	public class CallContextSessionStore : AbstractDictStackSessionStore
	{
		/// <summary>
		/// Gets the dictionary.
		/// </summary>
		/// <returns></returns>
		protected override IDictionary GetDictionary()
		{
			return CallContext.GetData(SlotKey) as IDictionary;
		}

		/// <summary>
		/// Stores the dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		protected override void StoreDictionary(IDictionary dictionary)
		{
			CallContext.SetData(SlotKey, dictionary);
		}

		/// <summary>
		/// Gets the IStatelessSession dictionary.
		/// </summary>
		/// <returns>A dictionary.</returns>
		protected override IDictionary GetStatelessSessionDictionary()
		{
			return CallContext.GetData(this.StatelessSessionSlotKey) as IDictionary;
		}

		/// <summary>
		/// Stores the IStatelessSession dictionary.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		protected override void StoreStatelessSessionDictionary(IDictionary dictionary)
		{
			CallContext.SetData(this.StatelessSessionSlotKey, dictionary);
		}
	}
}