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
	using System.Security.Cryptography.X509Certificates;
	using System.ServiceModel.Description;

	public class CertificateCredentials : AbstractCredentials
	{
		public CertificateCredentials()
		{
		}

		public CertificateCredentials(X509Certificate2 certificate)
		{
			Certificate = certificate;
		}

		public CertificateCredentials(FetchCertificate findCertificate)
		{
			FindCertificate = findCertificate;
		}

		public CertificateCredentials(FetchCertificateBySubject findCertificateBySubject)
		{
			FindCertificateBySubject = findCertificateBySubject;
		}

		public X509Certificate2 Certificate { get; set; }

		public FetchCertificate FindCertificate { get; set; }

		public FetchCertificateBySubject FindCertificateBySubject { get; set; }

		protected override void ConfigureCredentials(ClientCredentials credentials)
		{
			if (Certificate != null)
			{
				credentials.ClientCertificate.Certificate = Certificate;
			}
			else if (FindCertificateBySubject != null)
			{
				credentials.ClientCertificate.SetCertificate(
					FindCertificateBySubject.SubjectName, FindCertificateBySubject.StoreLocation,
					FindCertificateBySubject.StoreName);
			}
			else if (FindCertificate != null)
			{
				credentials.ClientCertificate.SetCertificate(
					FindCertificate.StoreLocation, FindCertificate.StoreName,
					FindCertificate.Criteria, FindCertificate.MatchesValue);
			}
		}
	} 
}
