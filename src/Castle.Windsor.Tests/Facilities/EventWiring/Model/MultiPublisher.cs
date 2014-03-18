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

namespace CastleTests.Facilities.EventWiring.Model
{
	using System;

	public class MultiPublisher
	{
		public void Trigger1()
		{
			if (Event1 != null)
			{
				Event1(this, new EventArgs());
			}
		}

		public void Trigger2()
		{
			if (Event2 != null)
			{
				Event2(this, new CustomEventArgs());
			}
		}

		public event PublishOneEventHandler Event1;

		public event PublishTwoEventHandler Event2;
	}
}