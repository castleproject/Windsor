using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.Services.Transaction;

namespace Castle.Facilities.AutoTx
{
	/// <summary>
	/// Storage for attributes found on transactional classes.
	/// </summary>
	public class TransactionMetaInfo : MarshalByRefObject
	{
		private readonly Dictionary<MethodInfo, TransactionAttribute> method2Att;
		private readonly HashSet<MethodInfo> injectMethods;
		private readonly Dictionary<MethodInfo, String> notTransactionalCache;
		private readonly object locker = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionMetaInfo"/> class.
		/// </summary>
		public TransactionMetaInfo()
		{
			method2Att = new Dictionary<MethodInfo, TransactionAttribute>();
			injectMethods = new HashSet<MethodInfo>();
			notTransactionalCache = new Dictionary<MethodInfo, String>();
		}

		#region MarshalByRefObject overrides

		/// <summary>
		/// Obtains a lifetime service object to control the lifetime policy for this instance.
		/// </summary>
		/// <returns>
		/// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"/> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> property.
		/// </returns>
		/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. 
		///                 </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/></PermissionSet>
		public override object InitializeLifetimeService()
		{
			return null;
		}

		#endregion

		///<summary>
		/// Adds a method info and the corresponding transaction attribute.
		///</summary>
		public void Add(MethodInfo method, TransactionAttribute attribute)
		{
			method2Att[method] = attribute;
		}

		/// <summary>
		/// Adds the method to the list of method which are going to have their
		/// transactions injected as a parameter.
		/// </summary>
		/// <param name="method"></param>
		public void AddInjection(MethodInfo method)
		{
			injectMethods.Add(method);
		}

		///<summary>
		/// Methods which needs transactions.
		///</summary>
		public IEnumerable<MethodInfo> Methods
		{
			get
			{
				// quicker than array: http://blogs.msdn.com/ricom/archive/2006/03/12/549987.aspx
				var methods = new List<MethodInfo>(method2Att.Count); 
				methods.AddRange(method2Att.Keys);
				return methods;
			}
		}

		public bool Contains(MethodInfo info)
		{
			lock (locker)
			{
				if (method2Att.ContainsKey(info)) return true;
				if (notTransactionalCache.ContainsKey(info)) return false;

				if (info.DeclaringType.IsGenericType || info.IsGenericMethod)
				{
					return IsGenericMethodTransactional(info);
				}

				return false;
			}
		}

		/// <summary>
		/// Gets whether the method should have its transaction injected.
		/// </summary>
		/// <param name="info">The method to inject for.</param>
		/// <returns>Whether to inject the transaction as a parameter into the method invocation.</returns>
		public bool ShouldInject(MethodInfo info)
		{
			return injectMethods.Contains(info);
		}

		public TransactionAttribute GetTransactionAttributeFor(MethodInfo methodInfo)
		{
			return method2Att[methodInfo];
		}

		private bool IsGenericMethodTransactional(MethodInfo info)
		{
			object[] atts = info.GetCustomAttributes(typeof(TransactionAttribute), true);

			if (atts.Length != 0)
			{
				Add(info, atts[0] as TransactionAttribute);
				return true;
			}
			else
			{
				notTransactionalCache[info] = string.Empty;
			}

			return false;
		}
	}
}