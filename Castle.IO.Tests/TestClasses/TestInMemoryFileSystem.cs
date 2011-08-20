using System;
using Castle.IO.FileSystems.InMemory;

namespace Castle.IO.Tests.TestClasses
{
	public class TestInMemoryFileSystem : InMemoryFileSystem
	{
		public TestInMemoryFileSystem()
		{
			CurrentDirectory = new Path(Environment.CurrentDirectory);
		}
	}
}