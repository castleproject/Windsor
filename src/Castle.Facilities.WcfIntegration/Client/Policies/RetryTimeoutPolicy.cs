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

namespace Castle.Facilities.WcfIntegration
{
    using System;
	using System.Threading;

    /// <summary>
    /// Policy to retry from a <see cref="T:System.TimeoutException" /> by performing an exponential backoff strategy.
    /// </summary>
    public class RetryTimeoutPolicy : AbstractWcfPolicy
    {
		public RetryTimeoutPolicy()
		{
			MaxRetries = 3;
			BackOff = new TimeSpan(0, 0, 30);
			MinBackOff = new TimeSpan(0, 0, 3);
			MaxBackOff = new TimeSpan(0, 0, 90);
		}

		public int MaxRetries { get; set; }

		public TimeSpan BackOff { get; set; }

		public TimeSpan MinBackOff { get; set; }

		public TimeSpan MaxBackOff { get; set; }

		/// <inheritdoc />
        public override void Apply(WcfInvocation wcfInvocation)
        {
			int retries = 0;
			Random random = null;

			while (true)
			{
				try
				{
					wcfInvocation.Proceed();
					return;
				}
				catch (TimeoutException)
				{
					if (retries++ >= MaxRetries)
						throw;
				}

				if (random == null)
					random = new Random();

				PerformBackoff(retries, random);

				wcfInvocation.Refresh(false);
			};
        }

		private void PerformBackoff(int retryCount, Random random)
		{
			int increment = (int)((Math.Pow(2, retryCount - 1) - 1) * 
				            random.Next((int)(BackOff.TotalMilliseconds * 0.8), (int)(BackOff.TotalMilliseconds * 1.2)));
			int sleepMsec = (int)Math.Min(MinBackOff.TotalMilliseconds + increment, MaxBackOff.TotalMilliseconds);
			Thread.Sleep(sleepMsec);
		}
    }
}