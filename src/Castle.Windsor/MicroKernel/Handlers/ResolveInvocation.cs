﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Handlers
{
	using System;

	using Castle.MicroKernel.Context;

	public class ResolveInvocation
	{
		private bool decommissionRequired;
		private Action proceed;

		public ResolveInvocation(CreationContext context, bool instanceRequired)
		{
			Context = context;
			InstanceRequired = instanceRequired;
		}

		public Burden Burden { get; set; }

		public CreationContext Context { get; private set; }
		public bool InstanceRequired { get; private set; }

		public object ResolvedInstance { get; set; }

		internal bool DecommissionRequired
		{
			get { return decommissionRequired; }
		}

		public void Proceed()
		{
			proceed.Invoke();
		}

		public void RequireDecommission()
		{
			decommissionRequired = true;
		}

		internal void SetProceedDelegate(Action value)
		{
			proceed = value;
		}
	}
}