// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Proxy
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Castle.Core.Interceptor;

	/// <summary>
	/// Represents options to configure proxies.
	/// </summary>
	public class ProxyOptions
	{
		private List<Type> interfaceList;
		private List<IReference<object>> mixInList;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProxyOptions"/> class.
		/// </summary>
		public ProxyOptions()
		{
			UseSingleInterfaceProxy = false;
			OmitTarget = false;
		}

		/// <summary>
		/// Gets or sets the proxy hook.
		/// </summary>
		public IReference<IProxyHook> Hook { get; set; }

		/// <summary>
		/// Gets or sets the interceptor selector.
		/// </summary>
		public IReference<IInterceptorSelector> Selector { get; set; }

		/// <summary>
		/// Determines if the proxied component uses a target.
		/// </summary>
		public bool OmitTarget { get; set; }

		/// <summary>
		/// Determines if the proxied component can change targets.
		/// </summary>
		public bool AllowChangeTarget { get; set; }

		/// <summary>
		/// Determines if the proxied component should only include
		/// the service interface.
		/// </summary>
		public bool UseSingleInterfaceProxy { get; set; }

#if (!SILVERLIGHT)
		/// <summary>
		/// Determines if the interface proxied component should inherit 
		/// from <see cref="MarshalByRefObject"/>
		/// </summary>
		public bool UseMarshalByRefAsBaseClass { get; set; }

#endif

		/// <summary>
		/// Gets the additional interfaces to proxy.
		/// </summary>
		/// <value>The interfaces.</value>
		public Type[] AdditionalInterfaces
		{
			get
			{
				if (interfaceList != null)
				{
					return interfaceList.ToArray();
				}

				return new Type[0];
			}
		}

		/// <summary>
		/// Gets the mix ins to integrate.
		/// </summary>
		/// <value>The interfaces.</value>
		public IEnumerable<IReference<object>> MixIns
		{
			get
			{
				if (mixInList == null)
				{
					yield break;
				}

				foreach (var reference in this.mixInList)
				{
					yield return reference;
				}
			}
		}

		/// <summary>
		/// Adds the additional interfaces to proxy.
		/// </summary>
		/// <param name="interfaces">The interfaces.</param>
		public void AddAdditionalInterfaces(params Type[] interfaces)
		{
			if (interfaces == null)
			{
				throw new ArgumentNullException("interfaces");
			}

			if (interfaceList == null)
			{
				interfaceList = new List<Type>();
			}

			interfaceList.AddRange(interfaces);
		}

		/// <summary>
		/// Adds the additional mix ins to integrate.
		/// </summary>
		/// <param name="mixIns">The mix ins.</param>
		public void AddMixIns(params object[] mixIns)
		{
			if (mixIns == null)
			{
				throw new ArgumentNullException("mixIns");
			}

			if (mixInList == null)
			{
				mixInList = new List<IReference<object>>();
			}

			foreach (var mixIn in mixIns)
			{
				mixInList.Add(new InstanceReference<object>(mixIn));
			}
		}

		/// <summary>
		/// Adds the additional mix in to integrate.
		/// </summary>
		/// <param name="mixIn">The mix in.</param>
		public void AddMixinReference(IReference<object> mixIn)
		{
			if (mixIn == null)
			{
				throw new ArgumentNullException("mixIn");
			}

			if (mixInList == null)
			{
				mixInList = new List<IReference<object>>();
			}
			mixInList.Add(mixIn);
		}

		/// <summary>
		/// Equalses the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns>true if equal.</returns>
		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			var proxyOptions = obj as ProxyOptions;
			if (proxyOptions == null) return false;
			if (!Equals(this.Hook, proxyOptions.Hook)) return false;
			if (!Equals(this.Selector, proxyOptions.Selector)) return false;
			if (!Equals(this.UseSingleInterfaceProxy, proxyOptions.UseSingleInterfaceProxy)) return false;
			if (!Equals(this.OmitTarget, proxyOptions.OmitTarget)) return false;
			if (!AdditionalInterfacesAreEquals(proxyOptions)) return false;
			return MixInsAreEquals(proxyOptions);
		}

		/// <summary>
		/// Gets the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return 29 * base.GetHashCode()
				+ GetCollectionHashCode(interfaceList)
				+ GetCollectionHashCode(mixInList);
		}

		private bool AdditionalInterfacesAreEquals(ProxyOptions proxyOptions)
		{
			if (!Equals(interfaceList == null, proxyOptions.interfaceList == null)) return false;
			if (interfaceList == null) return true; //both are null, nothing more to check
			if (interfaceList.Count != proxyOptions.interfaceList.Count) return false;
			for (int i = 0; i < interfaceList.Count; ++i)
			{
				if (!proxyOptions.interfaceList.Contains(interfaceList[0])) return false;
			}
			return true;
		}

		private bool MixInsAreEquals(ProxyOptions proxyOptions)
		{
			if (!Equals(mixInList == null, proxyOptions.mixInList == null)) return false;
			if (mixInList == null) return true; //both are null, nothing more to check
			if (mixInList.Count != proxyOptions.mixInList.Count) return false;
			for (int i = 0; i < mixInList.Count; ++i)
			{
				if (!proxyOptions.mixInList.Contains(mixInList[0])) return false;
			}
			return true;
		}

		private int GetCollectionHashCode(IEnumerable items)
		{
			int result = 0;

			if (items == null) return result;

			foreach (object item in items)
			{
				result = 29 * result + item.GetHashCode();
			}

			return result;
		}
	}
}