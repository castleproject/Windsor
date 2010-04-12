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
namespace Castle.Windsor.Configuration.Interpreters.XmlProcessor
{
	using System.Collections.Generic;
	using System.Xml;

	public class DefaultXmlProcessorNodeList : IXmlProcessorNodeList
	{
		private IList<XmlNode> nodes;
		private int index = -1;

		public DefaultXmlProcessorNodeList(XmlNode node)
		{
			nodes = new List<XmlNode>();
			nodes.Add(node);
		}

		public DefaultXmlProcessorNodeList(IList<XmlNode> nodes)
		{
			this.nodes = nodes;
		}

		public DefaultXmlProcessorNodeList(XmlNodeList nodes)
		{
			this.nodes = CloneNodeList(nodes);
		}

		/// <summary>
		/// Make a shallow copy of the nodeList.
		/// </summary>
		/// <param name="nodeList">The nodeList to be copied.</param>
		/// <returns></returns>
		protected IList<XmlNode> CloneNodeList(XmlNodeList nodeList)
		{
			IList<XmlNode> nodes = new List<XmlNode>(nodeList.Count);

			foreach (XmlNode node in nodeList)
			{
				nodes.Add(node);
			}

			return nodes;
		}

		public XmlNode Current
		{
			get { return nodes[index]; }
		}

		public bool HasCurrent
		{
			get { return index < nodes.Count; }
		}

		public bool MoveNext()
		{
			return ++index < nodes.Count;
		}

		public int CurrentPosition
		{
			get { return index; }
			set { index = value; }
		}

		public int Count
		{
			get { return nodes.Count; }
		}
	}
}

#endif
