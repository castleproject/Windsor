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


#if !(MONO || SILVERLIGHT)

namespace Castle.MicroKernel.ComponentActivator
{
	using System;
	using System.Web;
	using System.Web.UI;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Attempts to dynamically load a UserControl by invoking Page.LoadControl.  
	///   There are two uses of this class.  
	///   <para>
	///     1) Add a component to the Kernel and add a VirtualPath attribute specifying 
	///     the relative path of the .ascx file for the associated UserControl. (easy)
	///   </para>
	///   <example>
	///     <code>
	///       &lt;component id="BasketView" 
	///       service="Castle.ShoppingCart.IBasketView, Castle.ShoppingCart"
	///       type="Castle.ShoppingCart.BasketView, Castle.ShoppingCart" 
	///       lifestyle="transient"
	///       virtualPath="~/Views/BasketView.ascx"
	///       /&gt;
	///     </code>
	///   </example>
	///   <para>
	///     2) Precompile a UserControl and add the pre-compiled class to the Kernel. (hard)  
	///     Has not been tested with proxies.
	///   </para>
	/// </summary>
	[Serializable]
	public class WebUserControlComponentActivator : DefaultComponentActivator
	{
		/// <summary>
		///   Initializes a new instance of the <see cref = "WebUserControlComponentActivator" /> class.
		/// </summary>
		/// <param name = "model">The model.</param>
		/// <param name = "kernel">The kernel.</param>
		/// <param name = "onCreation">The on creation.</param>
		/// <param name = "onDestruction">The on destruction.</param>
		public WebUserControlComponentActivator(ComponentModel model, IKernel kernel,
		                                        ComponentInstanceDelegate onCreation,
		                                        ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

		/// <summary>
		///   Creates the instance.
		/// </summary>
		/// <param name = "context">The context.</param>
		/// <param name = "constructor"></param>
		/// <param name = "arguments">The arguments.</param>
		/// <returns></returns>
		protected override object CreateInstance(CreationContext context, ConstructorCandidate constructor, object[] arguments)
		{
			object instance = null;

			var implType = Model.Implementation;

			var createProxy = Model.HasInterceptors;
			var createInstance = true;

			if (createProxy)
			{
				createInstance = Kernel.ProxyFactory.RequiresTargetInstance(Kernel, Model);
			}

			if (createInstance)
			{
				try
				{
					var currentContext = HttpContext.Current;
					if (currentContext == null)
					{
						throw new InvalidOperationException(
							"System.Web.HttpContext.Current is null.  WebUserControlComponentActivator can only be used in an ASP.Net environment.");
					}

					var currentPage = currentContext.Handler as Page;
					if (currentPage == null)
					{
						throw new InvalidOperationException("System.Web.HttpContext.Current.Handler is not of type System.Web.UI.Page");
					}

					var virtualPath = Model.Configuration.Attributes["VirtualPath"];
					if (!string.IsNullOrEmpty(virtualPath))
					{
						instance = currentPage.LoadControl(virtualPath);
					}
					else
					{
						instance = currentPage.LoadControl(implType, arguments);
					}
				}
				catch (Exception ex)
				{
					throw new ComponentActivatorException(
						"WebUserControlComponentActivator: could not instantiate " + Model.Implementation.FullName, ex);
				}
			}

			if (createProxy)
			{
				try
				{
					instance = Kernel.ProxyFactory.Create(Kernel, instance, Model, context, arguments);
				}
				catch (Exception ex)
				{
					throw new ComponentActivatorException("ComponentActivator: could not proxy " + Model.Implementation.FullName, ex);
				}
			}

			return instance;
		}
	}
}

#endif