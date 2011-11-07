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

namespace Castle.Facilities.WcfIntegration.Internal
{
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.Xml;

	internal static class WcfBindingUtils
	{
		public static void ConfigureQuotas(Binding binding, int quotaSize)
		{
			if (binding is BasicHttpBinding)
			{
				var basicHttpBinding = (BasicHttpBinding)binding;
				basicHttpBinding.MaxBufferSize = quotaSize;
				basicHttpBinding.MaxBufferPoolSize = quotaSize;
				basicHttpBinding.MaxReceivedMessageSize = quotaSize;
				ConfigureQuotas(basicHttpBinding.ReaderQuotas, quotaSize);
			}
			else if (binding is WSHttpBindingBase)
			{
				var wsHttpBinding = (WSHttpBindingBase)binding;
				wsHttpBinding.MaxBufferPoolSize = quotaSize;
				wsHttpBinding.MaxReceivedMessageSize = quotaSize;
				ConfigureQuotas(wsHttpBinding.ReaderQuotas, quotaSize);
			}
			else if (binding is WebHttpBinding)
			{
				var webHttpBinding = (WebHttpBinding)binding;
				webHttpBinding.MaxBufferSize = quotaSize;
				webHttpBinding.MaxBufferPoolSize = quotaSize;
				webHttpBinding.MaxReceivedMessageSize = quotaSize;
				ConfigureQuotas(webHttpBinding.ReaderQuotas, quotaSize);
			}
			else if (binding is NetTcpBinding)
			{
				var netTcpBinding = (NetTcpBinding)binding;
				netTcpBinding.MaxBufferSize = quotaSize;
				netTcpBinding.MaxBufferPoolSize = quotaSize;
				netTcpBinding.MaxReceivedMessageSize = quotaSize;
				ConfigureQuotas(netTcpBinding.ReaderQuotas, quotaSize);
			}
		}

		private static void ConfigureQuotas(XmlDictionaryReaderQuotas readerQuotas, int quotaSize)
		{
			readerQuotas.MaxArrayLength = quotaSize;
			readerQuotas.MaxDepth = quotaSize;
			readerQuotas.MaxStringContentLength = quotaSize;
			readerQuotas.MaxBytesPerRead = quotaSize;
			readerQuotas.MaxNameTableCharCount = quotaSize;
		}
	}
}
