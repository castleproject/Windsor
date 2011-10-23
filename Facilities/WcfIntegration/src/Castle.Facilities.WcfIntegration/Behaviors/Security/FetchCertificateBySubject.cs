
using System.Security.Cryptography.X509Certificates;

namespace Castle.Facilities.WcfIntegration.Behaviors
{
	public class FetchCertificateBySubject
	{
		public FetchCertificateBySubject(string subjectName)
		{
			SubjectName = subjectName;
			StoreLocation = StoreLocation.LocalMachine;
			StoreName = StoreName.My;
		}

		public string SubjectName { get; private set; }

		public StoreLocation StoreLocation { get; set; }

		public StoreName StoreName { get; set;  }

		public static implicit operator AbstractCredentials(FetchCertificateBySubject finder)
		{
			return new CertificateCredentials(finder);
		}
	}
}
