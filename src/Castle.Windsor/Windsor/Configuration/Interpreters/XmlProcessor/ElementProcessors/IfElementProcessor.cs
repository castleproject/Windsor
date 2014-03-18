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

	public class IfElementProcessor : AbstractStatementElementProcessor
	{
		public override String Name
		{
			get { return "if"; }
		}

		public override void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine)
		{
			var element = nodeList.Current as XmlElement;

			var processContents = ProcessStatement(element, engine);

			if (processContents)
			{
				var fragment = CreateFragment(element);
				MoveChildNodes(fragment, element);
				engine.DispatchProcessAll(new DefaultXmlProcessorNodeList(fragment.ChildNodes));
				ReplaceItself(fragment, element);
			}
			else
			{
				RemoveItSelf(element);
			}
		}
	}
}

#endif