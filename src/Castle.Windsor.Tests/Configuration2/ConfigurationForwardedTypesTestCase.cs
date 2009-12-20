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

#if (!SILVERLIGHT)
namespace Castle.Windsor.Tests.Configuration2
{
	using System;
	using System.IO;

	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigurationForwardedTypesTestCase
	{

		[SetUp]
		public void SetUp()
		{

			var dir = ConfigHelper.ResolveConfigPath("Configuration2/");
			var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir + "config_with_forwarded_types.xml");
			container = new WindsorContainer(file);
		}

		private IWindsorContainer container;

		[Test]
		public void Component_with_forwarded_types()
		{
			var first = container.Resolve<ICommon>("hasForwards");
			var second = container.Resolve<ICommon2>();
			Assert.AreSame(first, second);
		}
	}
}
#endif