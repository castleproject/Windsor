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

	public class ChooseElementProcessor : AbstractStatementElementProcessor
	{
		private static readonly String OtherwiseElemName = "otherwise";
		private static readonly String WhenElemName = "when";

		public override String Name
		{
			get { return "choose"; }
		}

		public override void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine)
		{
			var element = nodeList.Current as XmlElement;

			var fragment = CreateFragment(element);

			foreach (XmlNode child in element.ChildNodes)
			{
				if (IgnoreNode(child))
				{
					continue;
				}

				var elem = GetNodeAsElement(element, child);

				var found = false;

				if (elem.Name == WhenElemName)
				{
					found = ProcessStatement(elem, engine);
				}
				else if (elem.Name == OtherwiseElemName)
				{
					found = true;
				}
				else
				{
					throw new XmlProcessorException("'{0} can not contain only 'when' and 'otherwise' elements found '{1}'", element.Name, elem.Name);
				}

				if (found)
				{
					if (elem.ChildNodes.Count > 0)
					{
						MoveChildNodes(fragment, elem);
						engine.DispatchProcessAll(new DefaultXmlProcessorNodeList(fragment.ChildNodes));
					}
					break;
				}
			}

			ReplaceItself(fragment, element);
		}
	}
}

#endif