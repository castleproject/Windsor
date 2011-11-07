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

namespace Castle.Facilities.NHibernateIntegration.Internal
{
	using System;
	using Services.Transaction;
	using ITransaction = NHibernate.ITransaction;

	/// <summary>
	/// Adapter to <see cref="IResource"/> so a
	/// NHibernate transaction can be enlisted within
	/// <see cref="Castle.Services.Transaction.ITransaction"/> instances.
	/// </summary>
	public class ResourceAdapter : IResource, IDisposable
	{
		private readonly ITransaction transaction;
		private readonly bool isAmbient;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceAdapter"/> class.
		/// </summary>
		/// <param name="transaction">The transaction.</param>
		/// <param name="isAmbient"></param>
		public ResourceAdapter(ITransaction transaction, bool isAmbient)
		{
			this.transaction = transaction;
			this.isAmbient = isAmbient;
		}

		/// <summary>
		/// Implementors should start the
		/// transaction on the underlying resource
		/// </summary>
		public void Start()
		{
			transaction.Begin();
		}

		/// <summary>
		/// Implementors should commit the
		/// transaction on the underlying resource
		/// </summary>
		public void Commit()
		{
			transaction.Commit();
		}

		/// <summary>
		/// Implementors should rollback the
		/// transaction on the underlying resource
		/// </summary>
		public void Rollback()
		{
			//HACK: It was supossed to only a test but it fixed the escalated tx rollback issue. not sure if 
			//		this the right way to do it (probably not).
			if (!isAmbient)
				transaction.Rollback();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			transaction.Dispose();
		}
	}
}