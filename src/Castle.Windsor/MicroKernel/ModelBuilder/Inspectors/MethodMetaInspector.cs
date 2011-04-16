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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   Base for inspectors that want configuration associated with methods.
	///   For each child a <see cref = "MethodMetaModel" /> is created
	///   and added to ComponentModel's methods collection
	/// </summary>
	/// <remarks>
	///   Implementors should override the <see cref = "ObtainNodeName" /> return
	///   the name of the node to be inspected. For example:
	///   <code>
	///     <![CDATA[
	///   <transactions>
	///     <method name="Save" transaction="requires" />
	///   </transactions>
	/// ]]>
	///   </code>
	/// </remarks>
	public abstract class MethodMetaInspector : IContributeComponentModelConstruction
	{
		private static readonly BindingFlags AllMethods =
			BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Instance | BindingFlags.Static |
			BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn;

		private ITypeConverter converter;

		protected virtual bool ShouldUseMetaModel
		{
			get { return false; }
		}

		protected abstract String ObtainNodeName();

		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}

			if (model.Configuration == null || model.Implementation == null)
			{
				return;
			}

			var methodsNode = model.Configuration.Children[ObtainNodeName()];

			if (methodsNode == null)
			{
				return;
			}

			EnsureHasReferenceToConverter(kernel);

			foreach (var methodNode in methodsNode.Children)
			{
				var name = methodNode.Name;

				if ("method".Equals(name))
				{
					name = methodNode.Attributes["name"];
				}

				AssertNameIsNotNull(name, model);

				var metaModel = new MethodMetaModel(methodNode);

				if (IsValidMeta(model, metaModel))
				{
					if (ShouldUseMetaModel)
					{
						// model.MethodMetaModels.Add( metaModel );
					}

					var signature = methodNode.Attributes["signature"];

					var methods = GetMethods(model.Implementation, name, signature);

					if (methods.Count == 0)
					{
						var message = String.Format("The class {0} has tried to expose configuration for " +
						                            "a method named {1} which could not be found.", model.Implementation.FullName, name);

						throw new Exception(message);
					}

					ProcessMeta(model, methods, metaModel);

					if (ShouldUseMetaModel)
					{
						// RegisterMethodsForFastAccess(methods, signature, metaModel, model);
					}
				}
			}
		}

		protected virtual bool IsValidMeta(ComponentModel model, MethodMetaModel metaModel)
		{
			return true;
		}

		protected virtual void ProcessMeta(ComponentModel model, IList<MethodInfo> methods, MethodMetaModel metaModel)
		{
		}

		private void AssertNameIsNotNull(string name, ComponentModel model)
		{
			if (name == null)
			{
				var message = String.Format("The configuration nodes within 'methods' " +
				                            "for the component '{0}' does not have a name. You can either name " +
				                            "the node as the method name or provide an attribute 'name'", model.Name);

				throw new Exception(message);
			}
		}

		private Type[] ConvertSignature(string signature)
		{
			var parameters = signature.Split(';');

			var types = new List<Type>();

			foreach (var param in parameters)
			{
				try
				{
					types.Add(converter.PerformConversion<Type>(param));
				}
				catch (Exception)
				{
					var message = String.Format("The signature {0} contains an entry type {1} " +
					                            "that could not be converted to System.Type. Check the inner exception for " +
					                            "details", signature, param);

					throw new Exception(message);
				}
			}

			return types.ToArray();
		}

		private void EnsureHasReferenceToConverter(IKernel kernel)
		{
			if (converter != null)
			{
				return;
			}

			converter = (ITypeConverter)
			            kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
		}

		private IList<MethodInfo> GetMethods(Type implementation, String name, String signature)
		{
			if (string.IsNullOrEmpty(signature))
			{
				var allmethods = implementation.GetMethods(AllMethods);

				var methods = new List<MethodInfo>();

				foreach (var method in allmethods)
				{
					if (CultureInfo.InvariantCulture.CompareInfo.Compare(method.Name, name, CompareOptions.IgnoreCase) == 0)
					{
						methods.Add(method);
					}
				}

				return methods;
			}
			else
			{
				var methodInfo = implementation.GetMethod(name, AllMethods, null, ConvertSignature(signature), null);

				if (methodInfo == null)
				{
					return new MethodInfo[0];
				}

				return new List<MethodInfo> { methodInfo };
			}
		}
	}
}