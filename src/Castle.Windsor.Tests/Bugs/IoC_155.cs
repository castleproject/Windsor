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

namespace Castle.Windsor.Tests.Bugs
{
	using System;
	using Core.Resource;
	using NUnit.Framework;
	using Windsor.Configuration.Interpreters;

	[TestFixture]
	public class IoC_155
	{
		public interface IService { }

		public class Service : IService { }

		[Test]
		public void Type_not_implementing_service_should_throw()
		{
			Assert.Throws<Exception>(() =>
				new WindsorContainer(
					new XmlInterpreter(
						new StaticContentResource(
							@"<castle>
<components>
    <component id=""svc""
        service=""Castle.Windsor.Tests.Bugs.IoC_155+Service, Castle.Windsor.Tests""
        type=""Castle.Windsor.Tests.Bugs.IoC_155+IService, Castle.Windsor.Tests""/>
</components>
</castle>"))));
		}
	}
}

#endif
