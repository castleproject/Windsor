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

#if !SILVERLIGHT
namespace Castle.Windsor.Tests
{
	using System.Collections.Generic;
	using System.Linq;

	using Castle.XmlFiles;

	using NUnit.Framework;

	[TestFixture]
	public class ServiceOverridesStackOverflowTestCase
	{
		[Test]
		public void Should_not_StackOverflow()
		{
			var container = new WindsorContainer()
				.Install(Castle.Windsor.Installer.Configuration.FromXml(Xml.Embedded("channel1.xml")));

			var channel = container.Resolve<MessageChannel>("MessageChannel1");
			var array = channel.RootDevice.Children.ToArray();

			Assert.AreSame(channel.RootDevice, container.Resolve<IDevice>("device1"));
			Assert.AreEqual(2, array.Length);
			Assert.AreSame(array[0], container.Resolve<IDevice>("device2"));
			Assert.AreSame(array[1], container.Resolve<IDevice>("device3"));
		}
	}

	public class MessageChannel
	{
		private readonly IDevice rootDevice;

		public MessageChannel(IDevice root)
		{
			rootDevice = root;
		}

		public IDevice RootDevice
		{
			get { return rootDevice; }
		}
	}

	public interface IDevice
	{
		MessageChannel Channel { get; }
		IEnumerable<IDevice> Children { get; }

	}

	public abstract class BaseDevice : IDevice
	{
		public abstract IEnumerable<IDevice> Children { get; }

		public MessageChannel Channel { get; set; }

	}

	public class TestDevice : BaseDevice
	{
		private readonly List<IDevice> children;

		public TestDevice()
		{
		}

		public TestDevice(IEnumerable<IDevice> theChildren)
		{
			children = new List<IDevice>(theChildren);
		}

		public override IEnumerable<IDevice> Children
		{
			get { return children; }
		}
	}
}
#endif