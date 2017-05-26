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

namespace Castle.Windsor.Configuration.Interpreters.XmlProcessor
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Castle.Core.Internal;
	using Castle.Core.Resource;
	using Castle.MicroKernel.SubSystems.Resource;
	using Castle.Windsor.Configuration.Interpreters.XmlProcessor.ElementProcessors;

	public class DefaultXmlProcessorEngine : IXmlProcessorEngine
	{
		private readonly IXmlNodeProcessor defaultElementProcessor;
		private readonly Regex flagPattern = new Regex(@"^(\w|_)+$");
		private readonly IDictionary<string, bool> flags = new Dictionary<string, bool>();

		private readonly IDictionary<XmlNodeType, IDictionary<string, IXmlNodeProcessor>> nodeProcessors =
			new Dictionary<XmlNodeType, IDictionary<string, IXmlNodeProcessor>>();

		private readonly IDictionary<string, XmlElement> properties = new Dictionary<string, XmlElement>();
		private readonly Stack<IResource> resourceStack = new Stack<IResource>();

		private readonly IResourceSubSystem resourceSubSystem;

		/// <summary>
		///   Initializes a new instance of the <see cref = "DefaultXmlProcessorEngine" /> class.
		/// </summary>
		/// <param name = "environmentName">Name of the environment.</param>
		public DefaultXmlProcessorEngine(string environmentName) : this(environmentName, new DefaultResourceSubSystem())
		{
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "DefaultXmlProcessorEngine" /> class.
		/// </summary>
		/// <param name = "environmentName">Name of the environment.</param>
		/// <param name = "resourceSubSystem">The resource sub system.</param>
		public DefaultXmlProcessorEngine(string environmentName, IResourceSubSystem resourceSubSystem)
		{
			AddEnvNameAsFlag(environmentName);
			this.resourceSubSystem = resourceSubSystem;
			defaultElementProcessor = new DefaultElementProcessor();
		}

		public void AddFlag(string flag)
		{
			flags[GetCanonicalFlagName(flag)] = true;
		}

		public void AddNodeProcessor(Type type)
		{
			if (type.Is<IXmlNodeProcessor>())
			{
				var processor = type.CreateInstance<IXmlNodeProcessor>();
				foreach (var nodeType in processor.AcceptNodeTypes)
				{
					RegisterProcessor(nodeType, processor);
				}
			}
			else
			{
				throw new XmlProcessorException("{0} does not implement {1} interface", type.FullName, typeof(IXmlNodeProcessor).FullName);
			}
		}

		public void AddProperty(XmlElement content)
		{
			properties[content.Name] = content;
		}

		/// <summary>
		///   Processes the element.
		/// </summary>
		/// <param name = "nodeList">The element.</param>
		/// <returns></returns>
		public void DispatchProcessAll(IXmlProcessorNodeList nodeList)
		{
			while (nodeList.MoveNext())
			{
				DispatchProcessCurrent(nodeList);
			}
		}

		/// <summary>
		///   Processes the element.
		/// </summary>
		/// <param name = "nodeList">The element.</param>
		/// <returns></returns>
		public void DispatchProcessCurrent(IXmlProcessorNodeList nodeList)
		{
			var processor = GetProcessor(nodeList.Current);

			if (processor != null)
			{
				processor.Process(nodeList, this);
			}
		}

		public XmlElement GetProperty(string key)
		{
			XmlElement property;
			if (!properties.TryGetValue(key, out property))
			{
				return null;
			}

			return property.CloneNode(true) as XmlElement;
		}

		public IResource GetResource(String uri)
		{
			IResource resource;
			if (resourceStack.Count > 0)
			{
				resource = resourceStack.Peek();
			}
			else
			{
				resource = null;
			}

#if FEATURE_URIMEMBERS
			var uriSchemeDelimiter = Uri.SchemeDelimiter;
#else
			var uriSchemeDelimiter = "://";
#endif

			if (uri.IndexOf(uriSchemeDelimiter) != -1)
			{
				if (resource == null)
				{
					return resourceSubSystem.CreateResource(uri);
				}

				return resourceSubSystem.CreateResource(uri, resource.FileBasePath);
			}

			// NOTE: what if resource is null at this point?
			if (resourceStack.Count > 0)
			{
				return resource.CreateRelative(uri);
			}

			throw new XmlProcessorException("Cannot get relative resource '" + uri + "', resource stack is empty");
		}

		public bool HasFlag(string flag)
		{
			return flags.ContainsKey(GetCanonicalFlagName(flag));
		}

		public bool HasProperty(String name)
		{
			return properties.ContainsKey(name);
		}

		public bool HasSpecialProcessor(XmlNode node)
		{
			return GetProcessor(node) != defaultElementProcessor;
		}

		public void PopResource()
		{
			resourceStack.Pop();
		}

		public void PushResource(IResource resource)
		{
			resourceStack.Push(resource);
		}

		public void RemoveFlag(string flag)
		{
			flags.Remove(GetCanonicalFlagName(flag));
		}

		private void AddEnvNameAsFlag(string environmentName)
		{
			if (environmentName != null)
			{
				AddFlag(environmentName);
			}
		}

		private string GetCanonicalFlagName(string flag)
		{
			flag = flag.Trim().ToLower();

			if (!flagPattern.IsMatch(flag))
			{
				throw new XmlProcessorException("Invalid flag name '{0}'", flag);
			}

			return flag;
		}

		private IXmlNodeProcessor GetProcessor(XmlNode node)
		{
			IDictionary<string, IXmlNodeProcessor> processors;
			if (!nodeProcessors.TryGetValue(node.NodeType, out processors))
			{
				return null;
			}

			// sometimes nodes with the same name will not accept a processor
			IXmlNodeProcessor processor;
			if (!processors.TryGetValue(node.Name, out processor) || !processor.Accept(node))
			{
				if (node.NodeType == XmlNodeType.Element)
				{
					processor = defaultElementProcessor;
				}
			}

			return processor;
		}

		private void RegisterProcessor(XmlNodeType type, IXmlNodeProcessor processor)
		{
			IDictionary<string, IXmlNodeProcessor> typeProcessors;
			if (!nodeProcessors.TryGetValue(type, out typeProcessors))
			{
				typeProcessors = new Dictionary<string, IXmlNodeProcessor>();
				nodeProcessors[type] = typeProcessors;
			}

			if (typeProcessors.ContainsKey(processor.Name))
			{
				throw new XmlProcessorException("There is already a processor register for {0} with name {1} ", type, processor.Name);
			}

			typeProcessors.Add(processor.Name, processor);
		}
	}
}

#endif