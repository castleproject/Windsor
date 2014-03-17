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


#if(!SILVERLIGHT)

namespace Castle.Windsor.Configuration.Interpreters.XmlProcessor.ElementProcessors
{
	using System;
	using System.Xml;

	public class IncludeElementProcessor : AbstractXmlNodeProcessor
	{
		public override String Name
		{
			get { return "include"; }
		}

		/// <summary>
		///   Accepts the specified node.
		///   Check if node has the same name as the processor and the node.NodeType
		///   is in the AcceptNodeTypes List
		///   NOTE: since the BatchRegistrationFacility already uses an include
		///   element we will distinguish between both by looking for the presence of an uri attribute
		///   we should revisit this later by using xml-namespaces
		/// </summary>
		/// <param name = "node">The node.</param>
		/// <returns></returns>
		public override bool Accept(XmlNode node)
		{
			return node.Attributes.GetNamedItem("uri") != null && base.Accept(node);
		}

		public override void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine)
		{
			var element = nodeList.Current as XmlElement;

			var result = ProcessInclude(element, element.GetAttribute("uri"), engine);

			ReplaceItself(result, element);
		}

		public XmlNode ProcessInclude(XmlElement element, String includeUri, IXmlProcessorEngine engine)
		{
			XmlDocumentFragment frag = null;

			if (includeUri == null)
			{
				throw new ConfigurationProcessingException(
					String.Format("Found an include node without an 'uri' attribute: {0}", element.BaseURI));
			}

			var uriList = includeUri.Split(',');
			frag = CreateFragment(element);

			foreach (var uri in uriList)
			{
				using (var resource = engine.GetResource(uri))
				{
					var doc = new XmlDocument();

					try
					{
						using (var stream = resource.GetStreamReader())
						{
							doc.Load(stream);
						}
					}
					catch (Exception ex)
					{
						throw new ConfigurationProcessingException(
							String.Format("Error processing include node: {0}", includeUri), ex);
					}

					engine.PushResource(resource);

					engine.DispatchProcessAll(new DefaultXmlProcessorNodeList(doc.DocumentElement));

					engine.PopResource();

					if (element.GetAttribute("preserve-wrapper") == "yes")
					{
						AppendChild(frag, doc.DocumentElement);
					}
					else
					{
						AppendChild(frag, doc.DocumentElement.ChildNodes);
					}
				}
			}

			return frag;
		}
	}
}

#endif