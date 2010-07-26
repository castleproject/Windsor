namespace Castle.Samples.Uploader.Commands
{
	using System;

	public abstract class AbstractCommand :ICommand, IDisposable
	{
		private readonly ICommandHost host;

		protected AbstractCommand(ICommandHost host)
		{
			this.host = host;
		}

		public abstract string Name { get; }
		public abstract string Description { get; }
		public abstract bool Enabled { get; }

		public void Init()
		{
			host.Attach(this);
		}

		public void Dispose()
		{
			host.Detach(this);
		}

		public abstract void Execute();
	}
}