using System;

namespace Castle.Services.Transaction.Tests.vNext
{
	public class Thing
	{
		[Obsolete("NHibernate's c'tor")]
		protected Thing()
		{
		}

		public Thing(double val)
		{
			Value = val;
		}

		public Guid ID { get; protected set; }
		public double Value { get; set; }
	}
}