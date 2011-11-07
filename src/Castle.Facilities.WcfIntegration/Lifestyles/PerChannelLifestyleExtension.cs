namespace Castle.Facilities.WcfIntegration//in the default namespace so that it's visible out of the box
{
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;

	using Castle.Facilities.WcfIntegration.Lifestyles;

	public class PerChannelLifestyleExtension : IExtension<IContextChannel>
	{
		private IContextChannel channel;
		private readonly IDictionary<PerWcfSessionLifestyle, object> components = new Dictionary<PerWcfSessionLifestyle, object>();
		private bool used;

		/// <inheritDoc />
		public void Attach(IContextChannel owner)
		{
			if(used)
			{
				throw new InvalidOperationException("This instance was already used!");
			}

			if (channel != null)
			{
				throw new InvalidOperationException("Can't attach twice!");
			}

			used = true;
			channel = owner;
			channel.Faulted += Shutdown;
			channel.Closed += Shutdown;
		}

		private void Shutdown(object sender, EventArgs e)
		{
			channel.Extensions.Remove(this);
			foreach (var component in components)
			{
				component.Key.Release(component.Value);
			}
			components.Clear();
		}

		/// <inheritDoc />
		public void Detach(IContextChannel owner)
		{
			if (!used)
			{
				throw new InvalidOperationException("This instance was not used!");
			}
			if (channel == null)
			{
				throw new InvalidOperationException("Can't Detach twice or before attaching!");
			}
			channel.Faulted -= Shutdown;
			channel.Closed -= Shutdown;
			channel = null;
		}

		public object this[PerWcfSessionLifestyle manager]
		{
			get
			{
				object component;
				components.TryGetValue(manager, out component);
				return component;
			}
			set
			{
				components[manager] = value;
			}
		}
	}
}