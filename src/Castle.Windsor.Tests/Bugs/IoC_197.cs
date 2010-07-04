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

namespace Castle.Windsor.Tests.Bugs
{
#if !SILVERLIGHT
	using System.Collections.Generic;

	using Castle.Core.Resource;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;

	using NUnit.Framework;

	public class HasDictionaryDependency
	{
		public IDictionary<string, string> DictionaryProperty { get; set; }
	}

	[TestFixture]
	public class IoC_197
	{
		[Test]
		[Ignore("This is not supported. Perhaps it should.")]
		public void DictionaryAsParameterInXml()
		{
			var container =
				new WindsorContainer(
					new XmlInterpreter(
						new StaticContentResource(
							string.Format(
								@"<castle>
<components>
	<component lifestyle=""singleton""
		id=""Id.MyClass""
		type=""{0}"">
		<parameters>
			<DictionaryProperty>${{Id.dictionary}}</DictionaryProperty>
		</parameters>
	</component>

	<component id=""Id.dictionary"" lifestyle=""singleton""
						 service=""System.Collections.IDictionary, mscorlib""
						 type=""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]]""
						 >
		<parameters>
			<dictionary>
				<dictionary>
					<entry key=""string.key.1"">string value 1</entry>
					<entry key=""string.key.2"">string value 2</entry>
				</dictionary>
			</dictionary>
		</parameters>
	</component>
</components>
</castle>",
								typeof(HasDictionaryDependency).AssemblyQualifiedName))));

			var myInstance = container.Resolve<HasDictionaryDependency>();
			Assert.AreEqual(2, myInstance.DictionaryProperty.Count);
		}
	}
#endif
}