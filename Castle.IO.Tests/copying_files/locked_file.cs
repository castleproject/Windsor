using System;
using System.IO;
using Castle.IO;
using NUnit.Framework;
using OpenFileSystem.Tests.contexts;
using OpenWrap.Testing;
using OpenWrap.Tests.IO;

namespace OpenFileSystem.Tests.copying_files
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class locked_file<T> : files<T> where T : IFileSystem, new()
    {
        [TestCase(FileShare.None)]
        [TestCase(FileShare.Write)]
        [TestCase(FileShare.Delete)]
        public void cannot_be_copied(FileShare fileShare)
        {
            var tempFile = FileSystem.CreateTempFile();
            using(tempFile.Open(FileMode.Append, FileAccess.Write, fileShare))
            {
                Executing(() => tempFile.CopyTo(tempFile.Parent.GetFile(Guid.NewGuid().ToString())))
                    .ShouldThrow<IOException>();
            }
        }
}
    }
