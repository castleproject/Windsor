using System;
using System.Linq;
using NUnit.Framework;
using OpenFileSystem.Tests.contexts;
using OpenWrap.Testing;

namespace OpenFileSystem.Tests.directory_searches
{
    [TestFixture("c:\\path\\folder\\", "c:\\**\\folder")]
    [TestFixture("c:\\path\\folder\\", "c:\\path\\**\\folder")]
    [TestFixture("c:\\path\\folder\\", "c:\\path\\**\\**\\folder")]
    [TestFixture("c:\\path\\folder\\", "c:\\p*\\f*")]
    [TestFixture("c:\\path\\folder\\", "c:\\path\\f*")]
    [TestFixture("c:\\path\\folder\\", "**\\folder")]
    [TestFixture("c:\\path\\folder\\", "path\\**\\folder")]
    [TestFixture("c:\\path\\folder\\", "path\\**\\**\\folder")]
    [TestFixture("c:\\path\\folder\\", "p*\\f*")]
    [TestFixture("c:\\path\\folder\\", "path\\f*")]
    [TestFixture("c:\\path\\folder\\", "*\\f*")]
    [TestFixture("c:\\path\\folder\\", "path\\**\\*")]
    [TestFixture("c:\\path\\folder\\", "c:\\path\\folder\\")]
    public class recursive_directory_search : file_search_context
    {
        readonly string _existingDirectory;

        public recursive_directory_search(string directory, string searchSpec)
        {
            _existingDirectory = directory;
            given_directory(directory);

            when_searching_for_directories(searchSpec);
        }

        [Test]
        public void file_is_found()
        {
            Directories.ShouldHaveCountOf(1).First().Path.FullPath.ShouldBe(_existingDirectory);
        }
    }

}