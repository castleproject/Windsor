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

namespace Castle.Facilities.Remoting
{
#if (!SILVERLIGHT)
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Facilities.Remoting.CustomActivators;
	using Castle.MicroKernel;


	public class RemotingRegistry : MarshalByRefObject, IDisposable
	{
		private readonly IKernel kernel;
		private readonly Dictionary<string,ComponentModel> entries = new Dictionary<string, ComponentModel>();
		private readonly Dictionary<Type, ComponentModel> genericEntries = new Dictionary<Type, ComponentModel>();


		public RemotingRegistry(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public void AddComponentEntry(ComponentModel model)
		{
			if (model.Service.IsGenericType)
			{
				genericEntries[model.Service] = model;
				return;
			}

			entries[model.Name] = model;
		}

		public object CreateRemoteInstance(String key)
		{
			GetModel(key);

			return kernel.Resolve(key, new Arguments());
		}

		private ComponentModel GetModel(string key)
		{
			ComponentModel model;
			if (!entries.TryGetValue(key, out model))
			{
				throw new KernelException(String.Format("No remote/available component found for key {0}", key));
			}

			return model;
		}

		public void Publish(string key)
		{
			// Resolve first
			var mbr = (MarshalByRefObject)kernel.Resolve(key, new Arguments());

			// then get the model
			var model = GetModel(key);
			RemoteMarshallerActivator.Marshal(mbr, model);
		}

		/// <summary>
		/// Used in case of generics:
		/// </summary>
		/// <param name="serviceType"></param>
		/// <returns></returns>
		private ComponentModel GetModel(Type serviceType)
		{
			ComponentModel model;
			if (!genericEntries.TryGetValue(serviceType, out model))
			{
				throw new KernelException(String.Format("No remote/available component found for service type {0}", serviceType));
			}

			return model;
		}

		public object CreateRemoteInstance(Type serviceType)
		{
			return kernel.Resolve(serviceType);
		}


		public void Publish(Type serviceType)
		{
			// Resolve first
			var mbr = (MarshalByRefObject)kernel.Resolve(serviceType);

			// then get the model
			var model = GetModel(serviceType);

			RemoteMarshallerActivator.Marshal(mbr, model);
		}

		#region IDisposable Members

		public void Dispose()
		{
			entries.Clear();

			genericEntries.Clear();
		}

		#endregion
	}
#endif
}
