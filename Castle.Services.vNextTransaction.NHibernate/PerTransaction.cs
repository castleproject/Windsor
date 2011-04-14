using System;
using System.Diagnostics.Contracts;
using System.Web;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;

namespace Castle.Services.vNextTransaction.NHibernate
{
	[Serializable]
	public class PerTransaction : AbstractLifestyleManager
	{
		private readonly ITxManager _Manager;

		public PerTransaction(ITxManager manager)
		{
			Contract.Requires(manager != null);
			_Manager = manager;
		}

		// Methods
		public override void Dispose()
		{
		}

		internal void Evict(object instance)
		{
			using (new EvictionScope(this))
				Kernel.ReleaseComponent(instance);
		}

		public override bool Release(object instance)
		{
			return evicting && base.Release(instance);
		}

		public override object Resolve(CreationContext context)
		{
			if ()

		}

		// Nested Types
		private class EvictionScope : IDisposable
		{
			// Fields
			private readonly PerTransaction owner;

			// Methods
			public EvictionScope(PerTransaction owner)
			{
				this.owner = owner;
				this.owner.evicting = true;
			}

			public void Dispose()
			{
				owner.evicting = false;
			}
		}
	}
}