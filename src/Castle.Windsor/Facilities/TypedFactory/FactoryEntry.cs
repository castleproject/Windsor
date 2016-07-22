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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.ComponentModel;

	/// <summary>
	///   Legacy class from old impl. of the facility. Do not use it.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class FactoryEntry
	{
		private readonly String creationMethod;
		private readonly String destructionMethod;
		private readonly Type factoryInterface;
		private readonly String id;

		public FactoryEntry(String id, Type factoryInterface, String creationMethod, String destructionMethod)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}
			if (factoryInterface == null)
			{
				throw new ArgumentNullException("factoryInterface");
			}
			if (!factoryInterface.IsInterface)
			{
				throw new ArgumentException("factoryInterface must be an interface");
			}
			if (string.IsNullOrEmpty(creationMethod))
			{
				throw new ArgumentNullException("creationMethod");
			}

			this.id = id;
			this.factoryInterface = factoryInterface;
			this.creationMethod = creationMethod;
			this.destructionMethod = destructionMethod;
		}

		public String CreationMethod
		{
			get { return creationMethod; }
		}

		public String DestructionMethod
		{
			get { return destructionMethod; }
		}

		public Type FactoryInterface
		{
			get { return factoryInterface; }
		}

		public String Id
		{
			get { return id; }
		}
	}
}