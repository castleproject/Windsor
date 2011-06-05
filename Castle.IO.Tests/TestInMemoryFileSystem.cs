using System;
using OpenFileSystem.IO.FileSystems.InMemory;

namespace Castle.IO.Tests
{
	public class TestInMemoryFileSystem : InMemoryFileSystem
	{
		public TestInMemoryFileSystem()
		{
			this.CurrentDirectory = Environment.CurrentDirectory;
		}
	}
}