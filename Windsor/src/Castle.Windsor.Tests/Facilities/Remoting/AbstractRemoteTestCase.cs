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


#if (!SILVERLIGHT)

namespace CastleTests.Facilities.Remoting
{
	using System;
	using System.Globalization;
	using System.Reflection;

	using Castle.Windsor;
	using Castle.XmlFiles;

	using CastleTests.Components;

	using NUnit.Framework;

	[Serializable]
	public abstract class AbstractRemoteTestCase
	{
		protected AppDomain clientDomain;
		protected IWindsorContainer serverContainer;

		protected AppDomain serverDomain;

		protected abstract String GetServerConfigFile();

		[SetUp]
		public virtual void Init()
		{
			serverDomain = AppDomainFactory.Create("server");
			clientDomain = AppDomainFactory.Create("client");

			serverContainer = CreateRemoteContainer(serverDomain, GetServerConfigFile());
		}

		[TearDown]
		public virtual void Terminate()
		{
			serverContainer.Dispose();

			AppDomain.Unload(clientDomain);
			AppDomain.Unload(serverDomain);
		}

		protected IWindsorContainer CreateRemoteContainer(AppDomain domain, string configFile)
		{
			var resource = Xml.EmbeddedPath("RemotingFacility/" + configFile);

			var handle = domain.CreateInstance(
				typeof(WindsorContainer).Assembly.FullName,
				typeof(WindsorContainer).FullName, false, BindingFlags.Instance | BindingFlags.Public, null,
				new object[] { resource },
				CultureInfo.InvariantCulture, null
#if DOTNET35
				,null
#endif
				);

			return (IWindsorContainer)handle.Unwrap();
		}

		protected IWindsorContainer GetRemoteContainer(AppDomain domain, String configFile)
		{
			var resource = Xml.EmbeddedPath("RemotingFacility/" + configFile);
			var handle = domain.CreateInstance(
				typeof(ContainerPlaceHolder).Assembly.FullName,
				typeof(ContainerPlaceHolder).FullName, false, BindingFlags.Instance | BindingFlags.Public, null,
				new object[] { resource },
				CultureInfo.InvariantCulture, null
#if DOTNET35
				,null
#endif
				);
			var holder = handle.Unwrap() as ContainerPlaceHolder;

			return holder.Container;
		}
	}

	public class ContainerPlaceHolder : MarshalByRefObject
	{
		private static IWindsorContainer container;

		public ContainerPlaceHolder(string configFile)
		{
			if (container == null)
			{
				container = new WindsorContainer(configFile);
			}
		}

		public IWindsorContainer Container
		{
			get { return container; }
		}
	}
}

#endif