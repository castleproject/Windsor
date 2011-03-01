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

namespace Castle.Windsor.Tests.Components
{
	using System;
	using System.ComponentModel;

	public class SimpleServiceSupportInitialize : ISimpleService, ISupportInitialize
	{
		private bool initBegun;
		private bool initEnded;

		public bool InitBegun
		{
			get { return initBegun; }
		}

		public bool InitEnded
		{
			get { return initEnded; }
		}

		public void Operation()
		{
		}

		public void BeginInit()
		{
			if (initEnded)
			{
				throw new InvalidOperationException("Can't Begin init after it ended");
			}
			initBegun = true;
		}

		public void EndInit()
		{
			if (initBegun == false)
			{
				throw new InvalidOperationException("Can't End init before it begins");
			}
			initEnded = true;
		}
	}
}