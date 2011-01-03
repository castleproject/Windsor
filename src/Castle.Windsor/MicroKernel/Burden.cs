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

namespace Castle.MicroKernel
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;

	public delegate void BurdenReleased(Burden burden);

	public class Burden
	{
		private readonly IHandler handler;

		private object instance;
		private List<Burden> items;

		internal Burden(IHandler handler, bool requiresDecommission, bool trackedExternally)
		{
			this.handler = handler;
			TrackedExternally = trackedExternally;
			RequiresDecommission = requiresDecommission || Model.Lifecycle.HasDecommissionConcerns;
		}

		public object Instance
		{
			get { return instance; }
		}

		public ComponentModel Model
		{
			get { return handler.ComponentModel; }
		}

		public bool RequiresDecommission { get; set; }

		/// <summary>
		///   If <c>true</c> requires release by <see cref = "IReleasePolicy" />. If <c>false</c>, the object has a well defined, detectable end of life (web-request end, disposal of the container etc), and will be released externally.
		/// </summary>
		public bool RequiresPolicyRelease
		{
			get { return TrackedExternally == false && RequiresDecommission; }
		}

		public bool TrackedExternally { get; set; }

		public void AddChild(Burden child)
		{
			if (items == null)
			{
				items = new List<Burden>(Model.Dependents.Length);
			}
			items.Add(child);

			if (child.RequiresDecommission)
			{
				RequiresDecommission = true;
			}
		}

		public bool Release()
		{
			if (handler.Release(this) == false)
			{
				return false;
			}

			Released(this);

			if (items != null)
			{
				items.ForEach(c => c.Release());
			}
			return true;
		}

		public void SetRootInstance(object instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			this.instance = instance;
		}

		public event BurdenReleased Released = delegate { };
	}
}