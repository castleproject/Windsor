using System.Linq;
using NUnit.Framework;

namespace Castle.IO.Tests.file_searches
{
    [TestFixture("c:\\path\\file.txt", "c:\\**\\file.txt")]
    [TestFixture("c:\\path\\file.txt", "c:\\path\\**\\file.txt")]
    [TestFixture("c:\\path\\file.txt", "c:\\path\\**\\**\\file.txt")]
    [TestFixture("c:\\path\\file.txt", "c:\\p*\\*.txt")]
    [TestFixture("c:\\path\\file.txt", "c:\\path\\*.txt")]
    [TestFixture("c:\\path\\file.txt", "**\\file.txt")]
    [TestFixture("c:\\path\\file.txt", "path\\**\\file.txt")]
    [TestFixture("c:\\path\\file.txt", "path\\**\\**\\file.txt")]
    [TestFixture("c:\\path\\file.txt", "p*\\*.txt")]
    [TestFixture("c:\\path\\file.txt", "path\\*.txt")]
    [TestFixture("c:\\path\\file.txt", "c:\\path\\file.txt")]
    [TestFixture("c:\\path\\file.txt", "c:\\path\\file.txt", "c:\\path\\")]
    public class recursive_file_search : file_search_context
    {
        readonly string existingFile;

        public recursive_file_search(string file, string searchSpec) : this(file, searchSpec, null)
        {
        }

        public recursive_file_search(string file, string searchSpec, string currentDirectory)
        {
            existingFile = file;
            if (currentDirectory != null)
                given_currentDirectory(currentDirectory);
            given_file(file);


            when_searching_for_files(searchSpec);
        }

        [Test]
        public void file_is_found()
        {
            Files.ShouldHaveCountOf(1).First().Path.FullPath.ShouldBe(existingFile);
        }
    }
}