using System;
using OpenFileSystem.IO.FileSystems.InMemory;

namespace OpenWrap.Tests.IO
{
	public class TestInMemoryFileSystem : InMemoryFileSystem
	{
		public TestInMemoryFileSystem()
		{
			this.CurrentDirectory = Environment.CurrentDirectory;
		}
	}
}