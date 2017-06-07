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

#if(!SILVERLIGHT)
namespace Castle.Windsor.Tests.XmlProcessor
{
	using System;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Castle.Windsor.Configuration.Interpreters;
	using Castle.Windsor.Configuration.Interpreters.XmlProcessor;
	using CastleTests;

	using NUnit.Framework;

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	[TestFixture]
	public class XmlProcessorTestCase
	{
		[Test]
		public void InvalidFiles()
		{
			var files = Directory.GetFiles(GetFullPath(), "Invalid*.xml");
			Assert.IsNotEmpty(files);

			foreach (var fileName in files)
			{
				var doc = GetXmlDocument(fileName);
				var processor = new XmlProcessor();

				Assert.Throws(typeof(ConfigurationProcessingException), () =>
					processor.Process(doc.DocumentElement));

			}
		}

		/// <summary>
		/// Runs the tests.
		/// </summary>
		[Test]
		public void RunTests()
		{
			var files = Directory.GetFiles(GetFullPath(), "*Test.xml");
			Assert.IsNotEmpty(files);

			foreach(var fileName in files)
			{

				if (fileName.EndsWith("PropertiesWithAttributesTest.xml"))
				{
					continue;
				}

				var doc = GetXmlDocument(fileName);

				var resultFileName = fileName.Substring(0, fileName.Length - 4) + "Result.xml";

				var resultDoc = GetXmlDocument(resultFileName);

				var processor = new XmlProcessor();

				try
				{
					var result = processor.Process(doc.DocumentElement);

					var resultDocStr = StripSpaces(resultDoc.OuterXml);
					var resultStr = StripSpaces(result.OuterXml);

					// Debug.WriteLine(resultDocStr);
					// Debug.WriteLine(resultStr);

					Assert.AreEqual(resultDocStr, resultStr);
				}
				catch(Exception e)
				{
					throw new Exception("Error processing " + fileName, e);
				}
			}
		}

		#region Helpers

		public XmlDocument GetXmlDocument(string fileName)
		{
			XmlDocument doc = new XmlDocument();

			string content = File.ReadAllText(fileName);
			doc.LoadXml(content);

			return doc;
		}

		private string StripSpaces(String xml)
		{
			return Regex.Replace(xml, "\\s+", "", RegexOptions.Compiled);
		}

		private string GetFullPath()
		{
			return Path.Combine(AppContext.BaseDirectory, ConfigHelper.ResolveConfigPath("XmlProcessor/TestFiles/"));
		}

		#endregion
	}
}
#endif