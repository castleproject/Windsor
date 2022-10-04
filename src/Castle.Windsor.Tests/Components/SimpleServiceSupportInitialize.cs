// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Components
{
	using System;
	using System.ComponentModel;

	public class SimpleServiceSupportInitialize : ISimpleService, ISupportInitialize
	{
		public bool InitBegun { get; private set; }
		public bool InitEnded { get; private set; }

		public void Operation()
		{
		}

		public void BeginInit()
		{
			if (InitEnded)
			{
				throw new InvalidOperationException("Can't Begin init after it ended");
			}
			InitBegun = true;
		}

		public void EndInit()
		{
			if (InitBegun == false)
			{
				throw new InvalidOperationException("Can't End init before it begins");
			}
			InitEnded = true;
		}
	}
}
