using System.IO;
using NUnit.Framework;

namespace Castle.IO.Tests.writing_content
{
    [TestFixture(typeof(TestInMemoryFileSystem))]
    [TestFixture(typeof(TestLocalFileSystem))]
    public class using_opencreate<T> : file_system_ctxt<T> where T : IFileSystem, new()
    {
        public using_opencreate()
        {
            file = given_temp_file();
            given_content(1);
            given_content(42);

        }

        void given_content(byte data)
        {
            file.Write(data, mode: FileMode.OpenOrCreate);
        }

        [Test]
        public void file_length_is_updated()
        {
            file.Size.ShouldBe(1);
        }

        [Test]
        public void file_content_is_written()
        {
            file.ShouldBe(42);
        }
        ITemporaryFile file;
    }
}