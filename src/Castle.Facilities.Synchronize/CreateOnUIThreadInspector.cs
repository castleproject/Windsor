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

namespace Castle.Facilities.Synchronize
{
	using System;
	using System.Configuration;
	using System.Runtime.Remoting;
	using System.Windows;
	using System.Windows.Forms;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.MicroKernel.Util;

	using Application = System.Windows.Application;

	/// <summary>
	///   Ensure that synchronized controls are always created on the main Ui thread.
	/// </summary>
	internal class CreateOnUIThreadInspector : IContributeComponentModelConstruction, IDisposable
	{
		private readonly IReference<IProxyGenerationHook> controlProxyHook;
		private readonly MarshalingControl marshalingControl;

		/// <summary>
		///   Initializes a new instance of the <see cref = "CreateOnUIThreadInspector" /> class.
		/// </summary>
		/// <param name = "config">The config.</param>
		/// <param name = "converter"></param>
		public CreateOnUIThreadInspector(IConfiguration config, IConversionManager converter)
		{
			marshalingControl = new MarshalingControl();
			controlProxyHook = ObtainProxyHook(config, converter);
		}

		/// <summary>
		///   Processes <see cref = "Control" /> implementations.
		/// </summary>
		/// <param name = "kernel">The kernel.</param>
		/// <param name = "model">The model.</param>
		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			CreateOnUIThreadDelegate createOnUIThread = null;

			// Since controls created on different threads cannot be added to one
			// another, ensure that all controls are created on the main UI thread.

			if (model.Implementation.Is<Control>())
			{
				createOnUIThread = CreateOnWinformsUIThread;
			}
			else if (model.Implementation.Is<UIElement>())
			{
				createOnUIThread = CreateOnDispatcherUIThread;
			}

			if (createOnUIThread != null)
			{
				ConfigureProxyOptions(model);
				model.ExtendedProperties[Constants.CreateOnUIThead] = createOnUIThread;

				if (model.CustomComponentActivator != null)
				{
					model.ExtendedProperties[Constants.CustomActivator] = model.CustomComponentActivator;
				}
				model.CustomComponentActivator = typeof(CreateOnUIThreadActivator);
			}
		}

		private void ConfigureProxyOptions(ComponentModel model)
		{
			if (controlProxyHook == null)
			{
				return;
			}
			var proxyOptions = model.ObtainProxyOptions();
			proxyOptions.Hook = controlProxyHook;
		}

		private object CreateOnWinformsUIThread(CreationContextDelegate performCreation, CreationContext context)
		{
			if (marshalingControl.InvokeRequired)
			{
				return marshalingControl.Invoke((CreateOnUIThreadDelegate)CreateOnWinformsUIThread, performCreation, context);
			}

			var component = performCreation(context);
			var control = (Control)GetUnproxiedInstance(component);
			if (control.Handle == IntPtr.Zero)
			{
				throw new InvalidOperationException("The WinForms control handle could not be obtained");
			}
			return component;
		}

		/// <summary>
		///   Releases the marshalling control.
		/// </summary>
		void IDisposable.Dispose()
		{
			if (marshalingControl != null)
			{
				marshalingControl.Dispose();
			}
		}

		private static object CreateOnDispatcherUIThread(CreationContextDelegate performCreation, CreationContext context)
		{
			var application = Application.Current;

			if (application != null && application.CheckAccess() == false)
			{
				return application.Dispatcher.Invoke((CreateOnUIThreadDelegate)CreateOnDispatcherUIThread, performCreation, context);
			}

			return performCreation(context);
		}

		private static object GetUnproxiedInstance(object instance)
		{
			if (!RemotingServices.IsTransparentProxy(instance))
			{
				var accessor = instance as IProxyTargetAccessor;

				if (accessor != null)
				{
					instance = accessor.DynProxyGetTarget();
				}
			}
			return instance;
		}

		private static IReference<IProxyGenerationHook> ObtainProxyHook(IConfiguration config, IConversionManager converter)
		{
			IProxyGenerationHook hook = null;
			if (config != null)
			{
				var hookAttrib = config.Attributes[Constants.ControlProxyHookAttrib];

				if (hookAttrib != null)
				{
					var hookComponent = ReferenceExpressionUtil.ExtractComponentName(hookAttrib);
					if (hookComponent != null)
					{
						return new ComponentReference<IProxyGenerationHook>(hookComponent);
					}

					var hookType = converter.PerformConversion<Type>(hookAttrib);

					if (hookType.Is<IProxyGenerationHook>() == false)
					{
						var message = String.Format("The specified controlProxyHook does " +
						                            "not implement the interface {1}. Type {0}",
						                            hookType.FullName, typeof(IProxyGenerationHook).FullName);

						throw new ConfigurationErrorsException(message);
					}

					hook = hookType.CreateInstance<IProxyGenerationHook>();
				}
			}

			if (hook == null)
			{
				hook = SynchronizeProxyHook.Instance;
			}

			return new InstanceReference<IProxyGenerationHook>(hook);
		}

		private class MarshalingControl : Control
		{
			internal MarshalingControl()
			{
				Visible = false;
				SetTopLevel(true);
				CreateControl();
				CreateHandle();
			}

			protected override void OnLayout(LayoutEventArgs levent)
			{
			}

			protected override void OnSizeChanged(EventArgs e)
			{
			}
		}
	}
}