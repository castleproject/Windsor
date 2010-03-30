namespace Castle.Windsor.Tests.Bugs
{
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

		[Test,Ignore("This is not supported. Perhaps it should.")]
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

}