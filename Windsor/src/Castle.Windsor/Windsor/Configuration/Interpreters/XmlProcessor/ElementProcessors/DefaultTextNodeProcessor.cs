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
	using System.Configuration;
	using System.Text.RegularExpressions;
	using System.Xml;

	public class DefaultTextNodeProcessor : AbstractXmlNodeProcessor
	{
		/// <summary>
		///   Properties names can contain a-zA-Z0-9_. 
		///   i.e. #!{ my_node_name } || #{ my.node.name }
		///   spaces are trimmed
		/// </summary>
		private static readonly Regex PropertyValidationRegExp = new Regex(@"(\#!?\{\s*((?:\w|\.)+)\s*\})", RegexOptions.Compiled);

		private static readonly XmlNodeType[] acceptNodes = new[] { XmlNodeType.CDATA, XmlNodeType.Text };

		public override XmlNodeType[] AcceptNodeTypes
		{
			get { return acceptNodes; }
		}

		public override String Name
		{
			get { return "#text"; }
		}

		public override void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine)
		{
			var node = nodeList.Current as XmlCharacterData;

			ProcessString(node, node.Value, engine);
		}

		/// <summary>
		///   Processes the string.
		/// </summary>
		/// <param name = "node">The node.</param>
		/// <param name = "value">The value.</param>
		/// <param name = "engine">The context.</param>
		public void ProcessString(XmlNode node, string value, IXmlProcessorEngine engine)
		{
			var fragment = CreateFragment(node);

			Match match;
			var pos = 0;
			while ((match = PropertyValidationRegExp.Match(value, pos)).Success)
			{
				if (pos < match.Index)
				{
					AppendChild(fragment, value.Substring(pos, match.Index - pos));
				}

				var propRef = match.Groups[1].Value; // #!{ propKey }
				var propKey = match.Groups[2].Value; // propKey

				{
				}
				var prop = engine.GetProperty(propKey);

				if (prop != null)
				{
					// When node has a parentNode (not an attribute)
					// we copy any attributes for the property into the parentNode
					if (node.ParentNode != null)
					{
						MoveAttributes(node.ParentNode as XmlElement, prop);
					}

					AppendChild(fragment, prop.ChildNodes);
				}
				else if (IsRequiredProperty(propRef))
				{
					// fallback to reading from appSettings
					var appSetting = ConfigurationManager.AppSettings[propKey];
					if (appSetting != null)
					{
						AppendChild(fragment, appSetting);
					}
					else
					{
						throw new XmlProcessorException(String.Format("Required configuration property {0} not found", propKey));
					}
				}

				pos = match.Index + match.Length;
			}

			// Appending anything left
			if (pos > 0 && pos < value.Length)
			{
				AppendChild(fragment, value.Substring(pos, value.Length - pos));
			}

			// we only process when there was at least one match
			// even when the fragment contents is empty since
			// that could mean that there was a match but the property
			// reference was a silent property
			if (pos > 0)
			{
				if (node.NodeType == XmlNodeType.Attribute)
				{
					node.Value = fragment.InnerText.Trim();
				}
				else
				{
					ReplaceNode(node.ParentNode, fragment, node);
				}
			}
		}

		private bool IsRequiredProperty(string propRef)
		{
			return propRef.StartsWith("#{");
		}

		private void MoveAttributes(XmlElement targetElement, XmlElement srcElement)
		{
			for (var i = srcElement.Attributes.Count - 1; i > -1; i--)
			{
				var importedAttr = ImportNode(targetElement, srcElement.Attributes[i]) as XmlAttribute;
				targetElement.Attributes.Append(importedAttr);
			}
		}
	}
}

#endif