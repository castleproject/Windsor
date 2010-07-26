namespace Castle.Samples.Uploader
{
	using System;
	using System.Windows.Forms;

	using Castle.Windsor;
	using Castle.Windsor.Installer;

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (var container = BootstrapContainer())
			{
				var mainForm = container.Resolve<Shell>();
				Application.Run(mainForm);
			}
		}

		private static IWindsorContainer BootstrapContainer()
		{
			return new WindsorContainer()
				.Install(FromAssembly.This());
		}
	}
}
