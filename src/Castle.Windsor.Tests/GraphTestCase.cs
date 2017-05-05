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
	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class GraphTestCase
	{
		private IKernel kernel;

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[Test]
		public void TopologicalSortOnComponents()
		{
			kernel.Register(Component.For(typeof(A)).Named("a"));
			kernel.Register(Component.For(typeof(B)).Named("b"));
			kernel.Register(Component.For(typeof(C)).Named("c"));

			var nodes = kernel.GraphNodes;

			Assert.IsNotNull(nodes);
			Assert.AreEqual(3, nodes.Length);

			var vertices = TopologicalSortAlgo.Sort(nodes);

			Assert.AreEqual("c", (vertices[0] as ComponentModel).Name);
			Assert.AreEqual("b", (vertices[1] as ComponentModel).Name);
			Assert.AreEqual("a", (vertices[2] as ComponentModel).Name);
		}
	}
}