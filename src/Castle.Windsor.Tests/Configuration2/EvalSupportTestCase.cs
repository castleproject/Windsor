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

#if !SILVERLIGHT // we do not support xml config on SL

namespace Castle.Windsor.Tests.Configuration2
{
	using System;

	using Castle.Windsor.Configuration.Interpreters;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class EvalSupportTestCase
	{
		[Test]
		public void AssertBaseDirectoryIsCorrectlyEvaluated()
		{
			var container = new WindsorContainer(new XmlInterpreter(ConfigHelper.ResolveConfigPath("Configuration2/eval_config.xml")), new CustomEnv(true));

			var prop = container.Resolve<ComponentWithStringProperty>("component");

			Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, prop.Name);
		}
	}
}

#endif