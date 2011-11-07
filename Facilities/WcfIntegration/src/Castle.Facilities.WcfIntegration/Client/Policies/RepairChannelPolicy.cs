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
    using System.ServiceModel;
    using System.ServiceModel.Security;

    /// <summary>
    /// Policy to recover from a <see cref="T:System.ServiceModel.CommunicationException" />
    /// by repairing the channel and trying again.  This policy will handle situations in which a
	/// connection has been reset on the server which invalidates the the client channel.
    /// </summary>
    public class RepairChannelPolicy : AbstractWcfPolicy, IWcfPolicy
    {
        /// <inheritdoc />
        public override void Apply(WcfInvocation wcfInvocation)
        {
            var refresh = false;

            try
            {
                wcfInvocation.Refresh().Proceed();
            }
            catch (ChannelTerminatedException)
            {
                refresh = true;
            }
            catch (CommunicationObjectFaultedException)
            {
                refresh = true;
            }
            catch (CommunicationObjectAbortedException)
            {
                refresh = true;
            }
            catch (MessageSecurityException)
            {
                refresh = true;
            }
            catch (CommunicationException exception)
            {
                if (exception.GetType() != typeof(CommunicationException))
                {
                    throw;
                }
                refresh = true;
            }

            if (refresh)
            {
                wcfInvocation.Refresh().Proceed();
            }
        }
    }
}

