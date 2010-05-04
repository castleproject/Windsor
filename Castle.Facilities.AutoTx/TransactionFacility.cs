// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

using System;
using Castle.MicroKernel.Facilities;
using Castle.Services.Transaction;
using Castle.Services.Transaction.IO;

namespace Castle.Facilities.AutoTx
{
	/// <summary>
	/// Augments the kernel to handle transactional components
	/// </summary>
	public class TransactionFacility : AbstractFacility
	{
		private bool _AllowAccessOutsideRootFolder = true;
		private string _RootFolder;

		public TransactionFacility()
		{
		}

		public TransactionFacility(bool allowAccessOutsideRootFolder,
		                           string rootFolder)
		{
			_AllowAccessOutsideRootFolder = allowAccessOutsideRootFolder;
			_RootFolder = rootFolder;
		}

		/// <summary>
		/// This triggers a new file adapter / directory adapter to be created.
		/// </summary>
		public bool AllowAccessOutsideRootFolder
		{
			get { return _AllowAccessOutsideRootFolder; }
			set { _AllowAccessOutsideRootFolder = value; }
		}

		public string RootFolder
		{
			get { return _RootFolder; }
			set { _RootFolder = value; }
		}

		/// <summary>
		/// Registers the interceptor component, the metainfo store and
		/// adds a contributor to the ModelBuilder
		/// </summary>
		protected override void Init()
		{
			AssertHasDirectories();
			Kernel.AddComponent("transaction.interceptor", typeof (TransactionInterceptor));
			Kernel.AddComponent("transaction.MetaInfoStore", typeof (TransactionMetaInfoStore));
			Kernel.AddComponent("directory.adapter.mappath", typeof (IMapPath), typeof (MapPathImpl));
			
			RegisterAdapters();

			Kernel.ComponentModelBuilder.AddContributor(new TransactionComponentInspector());

		}

		private void RegisterAdapters()
		{
			var directoryAdapter = new DirectoryAdapter(
				Kernel.Resolve<IMapPath>(),
				!_AllowAccessOutsideRootFolder,
				RootFolder);

			Kernel.AddComponentInstance("directory.adapter", typeof (IDirectoryAdapter), directoryAdapter);

			var fileAdapter = new FileAdapter(
				!_AllowAccessOutsideRootFolder,
				RootFolder);
			Kernel.AddComponentInstance("file.adapter", typeof (IFileAdapter), fileAdapter);

			if (Kernel.HasComponent(typeof(ITransactionManager)))
				fileAdapter.TxManager = directoryAdapter.TxManager = Kernel.Resolve<ITransactionManager>();
			else
				Kernel.ComponentRegistered += Kernel_ComponentRegistered;
				
		}

		void Kernel_ComponentRegistered(string key, Castle.MicroKernel.IHandler handler)
		{
			if (handler.ComponentModel.Service.IsAssignableFrom(typeof(ITransactionManager)))
			{
				((DirectoryAdapter)Kernel.Resolve<IDirectoryAdapter>()).TxManager = (ITransactionManager)Kernel[key];
				((FileAdapter)Kernel.Resolve<IFileAdapter>()).TxManager = (ITransactionManager)Kernel[key];
			}
		}

		public override void Dispose()
		{
			Kernel.ComponentRegistered -= Kernel_ComponentRegistered;
			base.Dispose();
		}


		private void AssertHasDirectories()
		{
			if (!_AllowAccessOutsideRootFolder && _RootFolder == null)
				throw new FacilityException("You have to specify a root directory.");
		}
	}
}