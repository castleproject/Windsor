// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;

	public class Burden
	{
		private bool childRequiresDecommission;
		private List<Burden> children;
		private IHandler handler;
		private object instance;

		private bool instanceRequiresDecommission;

		public bool GraphRequiresDecommission
		{
			get { return instanceRequiresDecommission || childRequiresDecommission; }
		}

		public object Instance
		{
			get { return instance; }
		}

		public ComponentModel Model
		{
			get { return handler.ComponentModel; }
		}

		public void AddChild(Burden child)
		{
			if (children == null)
			{
				children = new List<Burden>();
			}
			children.Add(child);

			if (child.GraphRequiresDecommission)
			{
				childRequiresDecommission = true;
			}
		}

		public bool Release(IReleasePolicy policy)
		{
			if (policy == null)
			{
				throw new ArgumentNullException("policy");
			}

			if (handler.Release(instance) == false)
			{
				return false;
			}

			if (children != null)
			{
				foreach (var child in children)
				{
					policy.Release(child.instance);
				}
			}
			return true;
		}

		public void SetRootInstance(object instance, IHandler handler, bool hasDecomission)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}

			this.instance = instance;
			this.handler = handler;
			instanceRequiresDecommission = hasDecomission;
		}
	}
}