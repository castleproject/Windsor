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

	public class UndefProcessingInstructionProcessor : AbstractStatementElementProcessor
	{
		public override XmlNodeType[] AcceptNodeTypes
		{
			get { return new[] { XmlNodeType.ProcessingInstruction }; }
		}

		public override String Name
		{
			get { return "undef"; }
		}

		public override void Process(IXmlProcessorNodeList nodeList, IXmlProcessorEngine engine)
		{
			var node = nodeList.Current as XmlProcessingInstruction;

			engine.RemoveFlag(node.Data);

			RemoveItSelf(node);
		}
	}
}

#endif