using System;
using System.IO;

namespace Castle.IO.FileSystems.Local
{
    public class TemporaryLocalFile : LocalFile, ITemporaryFile
    {
        public TemporaryLocalFile(string filePath, Func<DirectoryInfo, IDirectory> directoryFactory)
            : base(filePath, directoryFactory)
        {
        }

        ~TemporaryLocalFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Delete();
        }
    }
}