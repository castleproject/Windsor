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

namespace Castle.Facilities.WcfIntegration.Tests.Components
{
	using System.ServiceModel;

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class ServiceWithSession : IServiceWithSession
	{
		private readonly IOne one;
		private readonly ITwo two;
		public static int InstanceCount;

		public ServiceWithSession(IOne one, ITwo two)
		{
			this.one = one;
			this.two = two;
			InstanceCount++;
		}

		public void Initiating(string a)
		{
			one.Do(a);
		}

		public void Operation1(string a)
		{
			one.Do(a);
		}

		public void Operation2(string a)
		{
			two.Do(a);
		}

		public void Terminating()
		{
		}
	}
}