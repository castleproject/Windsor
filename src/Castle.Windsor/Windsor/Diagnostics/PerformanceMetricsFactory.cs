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

namespace Castle.Windsor.Diagnostics
{
#if !SILVERLIGHT
	using System.Diagnostics;

	public class PerformanceMetricsFactory : IPerformanceMetricsFactory
	{
		private const string CastleWindsorCategoryName = "Castle Windsor";
		private const string InstanesTrackedByTheReleasePolicyCounterName = "Instances tracked by the release policy";

		public PerformanceMetricsFactory()
		{
			if (PerformanceCounterCategory.Exists(CastleWindsorCategoryName) == false)
			{
				CreateWindsorCategoryAndCounters();
			}
		}

		public IPerformanceCounter CreateInstancesTrackedByReleasePolicyCounter(string name)
		{
			var counter = new PerformanceCounter(CastleWindsorCategoryName,
			                                     InstanesTrackedByTheReleasePolicyCounterName,
			                                     name,
			                                     readOnly: false) { RawValue = 0L };
			return new PerformanceCounterWrapper(counter);
		}

		private PerformanceCounterCategory CreateWindsorCategoryAndCounters()
		{
			return PerformanceCounterCategory.Create(CastleWindsorCategoryName,
			                                         "Performance counters published by the Castle Windsor container",
			                                         PerformanceCounterCategoryType.MultiInstance,
			                                         new CounterCreationDataCollection
			                                         {
			                                         	new CounterCreationData
			                                         	{
			                                         		CounterType = PerformanceCounterType.NumberOfItems32,
			                                         		CounterName = InstanesTrackedByTheReleasePolicyCounterName,
			                                         		CounterHelp = "List of instances tracked by the release policy in the container. " +
			                                         		              "Notice that does not include all alive objects tracked by the container, just the ones tracked by the policy."
			                                         	}
			                                         });
		}
	}
#endif
}