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

namespace Castle.Facilities.WcfIntegration.Behaviors
{
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Selectors;
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel;
	using System.ServiceModel.Description;
	using System.ServiceModel.Security;
	
	public abstract class AbstractCredentials : AbstractChannelFactoryAware
	{
		public X509RevocationMode? RevocationMode { get; set; }

        public X509CertificateValidationMode? CertificateValidationMode { get; set; }

		public X509CertificateValidator RemoteCertificateValidator { get; set; }

        public StoreLocation? TrustedStoreLocation { get; set; }

        public X509Certificate2 DefaultRemoteCertificate { get; set; }

		public Dictionary<Uri, X509Certificate2> ScopedRemoteCertificates { get; set; }

		protected abstract void ConfigureCredentials(ClientCredentials credentials);

		public override void Created(ChannelFactory channelFactory)
		{
			ConfigureCredentials(channelFactory.Credentials);

			ConfigureServiceAuthentication(channelFactory.Credentials.ServiceCertificate);
		}

		protected virtual void ConfigureServiceAuthentication(X509CertificateRecipientClientCredential service)
		{
			var authentication = service.Authentication;

			if (RevocationMode.HasValue)
				authentication.RevocationMode = RevocationMode.Value;

			if (CertificateValidationMode.HasValue)
				authentication.CertificateValidationMode = CertificateValidationMode.Value;

			if (RemoteCertificateValidator != null)
				authentication.CustomCertificateValidator = RemoteCertificateValidator;

			if (TrustedStoreLocation.HasValue)
				authentication.TrustedStoreLocation = TrustedStoreLocation.Value;

			if (DefaultRemoteCertificate != null)
				service.DefaultCertificate = DefaultRemoteCertificate;

			if (ScopedRemoteCertificates != null)
			{
				foreach (var certificateScope in ScopedRemoteCertificates)
                {
					service.ScopedCertificates.Add(certificateScope.Key, certificateScope.Value);         	
                }
			}
		}
	}
}
