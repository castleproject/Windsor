
using System.Security.Cryptography.X509Certificates;

namespace Castle.Facilities.WcfIntegration.Behaviors
{
	public class FetchCertificate
	{
		public FetchCertificate(X509FindType criteria, object match)
		{
			Criteria = criteria;
			MatchesValue = match;
			StoreLocation = StoreLocation.LocalMachine;
			StoreName = StoreName.My;
		}

		public X509FindType Criteria { get; private set; }

		public object MatchesValue { get; private set; }

		public StoreLocation StoreLocation { get; set; }

		public StoreName StoreName { get; set;  }

		public static implicit operator AbstractCredentials(FetchCertificate finder)
		{
			return new CertificateCredentials(finder);
		}
	}
}
