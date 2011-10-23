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

namespace Castle.MicroKernel.Tests
{
#if (!SILVERLIGHT)
	using System;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Security.Policy;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class SerializationTestCase
	{
		[Test]
		[Ignore("For some reason the deserialization of kernel members is resulting in null values")]
		public void KernelSerialization()
		{
			IKernel kernel = new DefaultKernel();
			kernel.Register(Component.For(typeof(CustomerImpl)).Named("key"));
			Assert.IsTrue(kernel.HasComponent("key"));

			var stream = new MemoryStream();
			var formatter = new BinaryFormatter();

			formatter.Serialize(stream, kernel);

			stream.Position = 0;

			var desKernel = (IKernel)formatter.Deserialize(stream);
			Assert.IsTrue(desKernel.HasComponent("key"));
		}

		[Test]
		[Ignore("To compile on Mono")]
		public void RemoteAccess()
		{
			var current = AppDomain.CurrentDomain;

			var otherDomain = AppDomain.CreateDomain(
				"other", new Evidence(current.Evidence), current.SetupInformation);

			try
			{
				var kernel = (IKernel)
				             otherDomain.CreateInstanceAndUnwrap(
				             	"Castle.Windsor", "Castle.MicroKernel.DefaultKernel");

				kernel.Register(Component.For(typeof(CustomerImpl)).Named("key"));
				Assert.IsTrue(kernel.HasComponent("key"));
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				AppDomain.Unload(otherDomain);
			}
		}

		[Test]
		[Ignore(@"Registration API is not serializable. Also ther are problems running this on mono2 profile")]
		public void RemoteAccessToComponentGraph()
		{
			var current = AppDomain.CurrentDomain;

			var otherDomain = AppDomain.CreateDomain(
				"other", new Evidence(current.Evidence), current.SetupInformation);

			try
			{
				var kernel = (IKernel)
				             otherDomain.CreateInstanceAndUnwrap(
				             	"Castle.Windsor", "Castle.MicroKernel.DefaultKernel");

				kernel.Register(Component.For(typeof(CustomerImpl)).Named("key"));
				Assert.IsTrue(kernel.HasComponent("key"));

				var nodes = kernel.GraphNodes;

				Assert.IsNotNull(nodes);
				Assert.AreEqual(1, nodes.Length);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				AppDomain.Unload(otherDomain);
			}
		}
	}
#endif
}