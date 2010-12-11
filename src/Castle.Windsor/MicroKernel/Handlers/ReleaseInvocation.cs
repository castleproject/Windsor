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

namespace Castle.MicroKernel.Handlers
{
	using System;

	public class ReleaseInvocation
	{
		private Action proceed;

		public ReleaseInvocation(Burden burden)
		{
			Burden = burden;
		}

		public Burden Burden { get; private set; }

		public object Instance
		{
			get { return Burden.Instance; }
		}

		public bool ReturnValue { get; set; }

		public void Proceed()
		{
			proceed.Invoke();
		}

		internal void SetProceedDelegate(Action value)
		{
			proceed = value;
		}
	}
}