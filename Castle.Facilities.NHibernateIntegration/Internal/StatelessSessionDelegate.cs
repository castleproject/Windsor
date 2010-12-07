namespace Castle.Facilities.NHibernateIntegration
{
	using System;
	using System.Data;
	using NHibernate;

	/// <summary>
	/// Proxies an IStatelessSession so the user cannot close a stateless session
	/// which is controlled by a transaction, or, when this is not the case, 
	/// make sure to remove the session from the storage.
	/// <seealso cref="ISessionStore"/>
	/// <seealso cref="ISessionManager"/>
	/// </summary>
	[Serializable]
	public class StatelessSessionDelegate : MarshalByRefObject, IStatelessSession
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StatelessSessionDelegate"/> class.
		/// </summary>
		/// <param name="canClose">if set to <c>true</c> [can close].</param>
		/// <param name="inner">The inner.</param>
		/// <param name="sessionStore">The session store.</param>
		public StatelessSessionDelegate(bool canClose, IStatelessSession inner, ISessionStore sessionStore)
		{
		}

		#region IStatelessSession delegation

		/// <summary>
		/// Returns the current ADO.NET connection associated with this instance.
		/// </summary>
		/// <remarks>
		/// If the session is using aggressive connection release (as in a
		/// CMT environment), it is the application's responsibility to
		/// close the connection returned by this call. Otherwise, the
		/// application should not close the connection.
		/// </remarks>
		public IDbConnection Connection
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Get the current Hibernate transaction.
		/// </summary>
		public ITransaction Transaction
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Begin a NHibernate transaction.
		/// </summary>
		public ITransaction BeginTransaction()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Close the stateless session and release the ADO.NET connection.
		/// </summary>
		public void Close()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new <see cref="T:NHibernate.ICriteria"/> instance, for the given entity class,
		/// or a superclass of an entity class. 
		/// </summary>
		/// <typeparam name="T">A class, which is persistent, or has persistent subclasses</typeparam>
		/// <returns>
		/// The <see cref="T:NHibernate.ICriteria"/>. 
		/// </returns>
		/// <remarks>
		/// Entities returned by the query are detached.
		/// </remarks>
		public ICriteria CreateCriteria<T>() where T : class
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new <see cref="T:NHibernate.ICriteria"/> instance, for the given entity class,
		/// or a superclass of an entity class, with the given alias. 
		/// </summary>
		/// <typeparam name="T">A class, which is persistent, or has persistent subclasses</typeparam>
		/// <param name="alias">The alias of the entity</param>
		/// <returns>
		/// The <see cref="T:NHibernate.ICriteria"/>. 
		/// </returns>
		/// <remarks>
		/// Entities returned by the query are detached.
		/// </remarks>
		public ICriteria CreateCriteria<T>(string alias) where T : class
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new <see cref="T:NHibernate.ICriteria"/> instance, for the given entity class,
		/// or a superclass of an entity class. 
		/// </summary>
		/// <param name="entityType">A class, which is persistent, or has persistent subclasses</param>
		/// <returns>
		/// The <see cref="T:NHibernate.ICriteria"/>. 
		/// </returns>
		/// <remarks>
		/// Entities returned by the query are detached.
		/// </remarks>
		public ICriteria CreateCriteria(Type entityType)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new <see cref="T:NHibernate.ICriteria"/> instance, for the given entity class,
		/// or a superclass of an entity class, with the given alias. 
		/// </summary>
		/// <param name="entityType">A class, which is persistent, or has persistent subclasses</param>
		/// <param name="alias">The alias of the entity</param>
		/// <returns>
		/// The <see cref="T:NHibernate.ICriteria"/>. 
		/// </returns>
		/// <remarks>
		/// Entities returned by the query are detached.
		/// </remarks>
		public ICriteria CreateCriteria(Type entityType, string alias)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new <see cref="T:NHibernate.ICriteria"/> instance, for the given entity name.
		/// </summary>
		/// <param name="entityName">The entity name. </param>
		/// <returns>
		/// The <see cref="T:NHibernate.ICriteria"/>. 
		/// </returns>
		/// <remarks>
		/// Entities returned by the query are detached.
		/// </remarks>
		public ICriteria CreateCriteria(string entityName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new <see cref="T:NHibernate.ICriteria"/> instance, for the given entity name,
		/// with the given alias.  
		/// </summary>
		/// <param name="entityName">The entity name. </param>
		/// <param name="alias">The alias of the entity</param>
		/// <returns>
		/// The <see cref="T:NHibernate.ICriteria"/>. 
		/// </returns>
		/// <remarks>
		/// Entities returned by the query are detached.
		/// </remarks>
		public ICriteria CreateCriteria(string entityName, string alias)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new instance of <tt>Query</tt> for the given HQL query string.
		/// </summary>
		/// <remarks>
		/// Entities returned by the query are detached.
		/// </remarks>
		public IQuery CreateQuery(string queryString)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create a new instance of <see cref="T:NHibernate.ISQLQuery"/> for the given SQL query string.
		/// Entities returned by the query are detached.
		/// </summary>
		/// <param name="queryString">a SQL query </param>
		/// <returns>
		/// The <see cref="T:NHibernate.ISQLQuery"/>
		/// </returns>
		public ISQLQuery CreateSQLQuery(string queryString)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Delete a entity. 
		/// </summary>
		/// <param name="entity">a detached entity instance </param>
		public void Delete(object entity)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Delete a entity. 
		/// </summary>
		/// <param name="entityName">The entityName for the entity to be deleted </param>
		/// <param name="entity">a detached entity instance </param>
		public void Delete(string entityName, object entity)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieve a entity. 
		/// </summary>
		/// <returns>
		/// a detached entity instance 
		/// </returns>
		public object Get(string entityName, object id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieve a entity.
		/// </summary>
		/// <returns>
		/// a detached entity instance
		/// </returns>
		public T Get<T>(object id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieve a entity, obtaining the specified lock mode. 
		/// </summary>
		/// <returns>
		/// a detached entity instance 
		/// </returns>
		public object Get(string entityName, object id, LockMode lockMode)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieve a entity, obtaining the specified lock mode. 
		/// </summary>
		/// <returns>
		/// a detached entity instance 
		/// </returns>
		public T Get<T>(object id, LockMode lockMode)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Obtain an instance of <see cref="T:NHibernate.IQuery"/> for a named query string defined in
		/// the mapping file.
		/// </summary>
		/// <remarks>
		/// The query can be either in <c>HQL</c> or <c>SQL</c> format.
		/// Entities returned by the query are detached.
		/// </remarks>
		public IQuery GetNamedQuery(string queryName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Insert a entity.
		/// </summary>
		/// <param name="entity">A new transient instance </param>
		/// <returns>
		/// the identifier of the instance 
		/// </returns>
		public object Insert(object entity)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Insert a row. 
		/// </summary>
		/// <param name="entityName">The entityName for the entity to be inserted </param>
		/// <param name="entity">a new transient instance </param>
		/// <returns>
		/// the identifier of the instance 
		/// </returns>
		public object Insert(string entityName, object entity)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Refresh the entity instance state from the database. 
		/// </summary>
		/// <param name="entity">The entity to be refreshed. </param>
		public void Refresh(object entity)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Refresh the entity instance state from the database. 
		/// </summary>
		/// <param name="entityName">The entityName for the entity to be refreshed. </param>
		/// <param name="entity">The entity to be refreshed.</param>
		public void Refresh(string entityName, object entity)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Refresh the entity instance state from the database. 
		/// </summary>
		/// <param name="entity">The entity to be refreshed. </param>
		/// <param name="lockMode">The LockMode to be applied.</param>
		public void Refresh(object entity, LockMode lockMode)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Refresh the entity instance state from the database. 
		/// </summary>
		/// <param name="entityName">The entityName for the entity to be refreshed. </param>
		/// <param name="entity">The entity to be refreshed. </param>
		/// <param name="lockMode">The LockMode to be applied. </param>
		public void Refresh(string entityName, object entity, LockMode lockMode)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Update a entity.
		/// </summary>
		/// <param name="entity">a detached entity instance </param>
		public void Update(object entity)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Update a entity.
		/// </summary>
		/// <param name="entityName">The entityName for the entity to be updated </param>
		/// <param name="entity">a detached entity instance </param>
		public void Update(string entityName, object entity)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDisposable delegation

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion
	
		/// <summary>
		/// Returns <see langword="true"/> if the supplied stateless sessions are equal, <see langword="false"/> otherwise.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static bool AreEqual(IStatelessSession left, IStatelessSession right)
		{
			return false;
		}
	}
}