using Castle.IO.FileSystems;
using Castle.IO.FileSystems.InMemory;
using Castle.IO.FileSystems.Local;
using Castle.IO.FileSystems.Local.Unix;
using Castle.IO.FileSystems.Local.Win32;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Castle.IO.Windsor
{
	public class IOInstaller : IWindsorInstaller
	{
		public const string Win32FileSystem = "Win32FileSystem";
		public const string UnixFileSystem = "UnixFileSystem";
		public const string InMemoryFileSystem = "InMemoryFileSystem";

		public void Install(IWindsorContainer c, IConfigurationStore store)
		{
			c.Register(
				Component.For<IFileSystem, AbstractFileSystem>()
					.ImplementedBy<Win32FileSystem>()
					.Named(Win32FileSystem)
					.LifeStyle.Singleton,

				Component.For<IFileSystem, AbstractFileSystem>()
					.ImplementedBy<UnixFileSystem>()
					.Named(UnixFileSystem)
					.LifeStyle.Singleton,

				Component.For<IFileSystem, AbstractFileSystem>()
					.ImplementedBy<InMemoryFileSystem>()
					.Named(InMemoryFileSystem)
					.LifeStyle.Singleton,

				Component.For<IFileSystemNotifier, FileSystemNotifier>()
					.ImplementedBy<LocalFileSystemNotifier>()
					.LifeStyle.Singleton
				);
		}
	}
}