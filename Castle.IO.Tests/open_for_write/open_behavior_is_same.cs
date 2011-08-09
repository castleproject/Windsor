using System;
using System.IO;
using Castle.IO.FileSystems.InMemory;
using Castle.IO.FileSystems.Local;
using NUnit.Framework;

namespace Castle.IO.Tests.open_for_write
{
	public class open_behavior_is_same : context
	{
		[Test]
		public void existing_file(
			[Values(FileMode.Append, FileMode.Create, FileMode.CreateNew, FileMode.Open, FileMode.OpenOrCreate, FileMode.Truncate)] FileMode mode,
			[Values(FileAccess.Read, FileAccess.ReadWrite, FileAccess.Write)]														FileAccess access,
			[Values(FileShare.Delete, FileShare.None, FileShare.Read, FileShare.ReadWrite, FileShare.Write)]						FileShare share)
		{
			var inMem = new InMemoryFileSystem();
			using (var memTempDir = inMem.CreateTempDirectory())
			using (var localTempDir = LocalFileSystem.Instance.CreateTempDirectory())
			{
				var memTempFile = memTempDir.GetFile(Guid.NewGuid().ToString()).MustExist();
				var localTempFile = localTempDir.GetFile(Guid.NewGuid().ToString()).MustExist();

				TestSameCore(memTempFile, localTempFile, mode, access, share);
			}
		}

		[Test]
		public void non_existing_file(
			[Values(FileMode.Append, FileMode.Create, FileMode.CreateNew, FileMode.Open, FileMode.OpenOrCreate, FileMode.Truncate)] FileMode mode,
			[Values(FileAccess.Read, FileAccess.ReadWrite, FileAccess.Write)] FileAccess access,
			[Values(FileShare.Delete, FileShare.None, FileShare.Read, FileShare.ReadWrite, FileShare.Write)] FileShare share)
		{
			var inMem = new InMemoryFileSystem();
			using (var memTempDir = inMem.CreateTempDirectory())
			using (var localTempDir = LocalFileSystem.Instance.CreateTempDirectory())
			{
				var memTempFile = memTempDir.GetFile(Guid.NewGuid().ToString());
				var localTempFile = localTempDir.GetFile(Guid.NewGuid().ToString());

				TestSameCore(memTempFile, localTempFile, mode, access, share);
			}
		}

		private void TestSameCore(IFile memTempFile, IFile localTempFile, FileMode mode, FileAccess access, FileShare share)
		{
			Stream memStream = null, localStream = null;
			Exception memException = null, localException = null;
			try
			{
				memStream = memTempFile.Open(mode, access, share);
			}
			catch (Exception e)
			{
				memException = e;
			}
			try
			{
				localStream = localTempFile.Open(mode, access, share);
			}
			catch (Exception e)
			{
				localException = e;
			}
			if (memException != null || localException != null)
			{
				if (memStream != null) memStream.Dispose();
				if (localStream != null) localStream.Dispose();

				var anyException = (memException ?? localException).GetType();

				Assert.That(memException, Is.InstanceOf(anyException), string.Format("In-Memory ex was {0}, but file was: {1}", 
					memException != null ? memException.ToString() : "NULL", 
					localException.ToString()));
				
				Assert.That(localException, Is.InstanceOf(anyException), string.Format("Local file ex was {0}, but in mem was: {1}.",
					localException != null ? localException.ToString() : "NULL", memException.ToString()));

				if (!(memException.GetType() == anyException && localException.GetType() == anyException))
					Console.WriteLine("Memory exception: " + (memException != null ? memException.GetType().Name : null) +
					                  " Local exception: " + (localException != null ? localException.GetType().Name : null));
				return;
			}
			if (memStream.CanWrite)
				memStream.WriteByte(99);
			if (localStream.CanWrite)
				localStream.WriteByte(99);
			memStream.Dispose();
			localStream.Dispose();

			using (memStream = memTempFile.OpenRead())
			using (localStream = localTempFile.OpenRead())
			{
				memStream.ReadToEnd().ShouldBe(localStream.ReadToEnd());
				localStream.Close();
				memStream.Close();
			}
		}
	}
}