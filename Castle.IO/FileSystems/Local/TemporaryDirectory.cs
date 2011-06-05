using System;
using System.Collections.Generic;

namespace Castle.IO.FileSystems.Local
{
    public class TemporaryDirectory : ITemporaryDirectory
    {

        public IDirectory UnderlyingDirectory { get; set; }

        public TemporaryDirectory(IDirectory unerlyingDirectory)
        {
            UnderlyingDirectory = unerlyingDirectory;
            if (!UnderlyingDirectory.Exists)
                UnderlyingDirectory.Create();
        }

        public IDirectory Create()
        {
            return UnderlyingDirectory.Create();
        }

        public IDirectory GetDirectory(string directoryName)
        {
            return UnderlyingDirectory.GetDirectory(directoryName);
        }

        public IFile GetFile(string fileName)
        {
            return UnderlyingDirectory.GetFile(fileName);
        }

        public IEnumerable<IFile> Files()
        {
            return UnderlyingDirectory.Files();
        }

        public IEnumerable<IDirectory> Directories()
        {
            return UnderlyingDirectory.Directories();
            
        }

        public IEnumerable<IFile> Files(string filter, SearchScope scope)
        {
            return UnderlyingDirectory.Files(filter, scope);
            
        }

        public IEnumerable<IDirectory> Directories(string filter, SearchScope scope)
        {
            return UnderlyingDirectory.Directories(filter, scope);
            
        }

        public bool IsHardLink
        {
            get { return UnderlyingDirectory.IsHardLink; }
        }

        public IDirectory LinkTo(string path)
        {
            return UnderlyingDirectory.LinkTo(path);
            
        }

        public IDirectory Target
        {
            get { return UnderlyingDirectory.Target; }
        }

        public IDisposable FileChanges(string filter, bool includeSubdirectories, Action<IFile> created, Action<IFile> modified, Action<IFile> deleted, Action<IFile> renamed)
        {
            return UnderlyingDirectory.FileChanges(filter, includeSubdirectories, created, modified, deleted, renamed);
        }


        public Path Path
        {
            get { return UnderlyingDirectory.Path; }
        }

        public IDirectory Parent
        {
            get { return UnderlyingDirectory.Parent; }
        }

        public IFileSystem FileSystem
        {
            get { return UnderlyingDirectory.FileSystem; }
        }

        public bool Exists
        {
            get { return UnderlyingDirectory.Exists; }
        }

        public string Name
        {
            get { return UnderlyingDirectory.Name; }
        }

        public void Delete()
        {
            UnderlyingDirectory.Delete();

        }

        public void CopyTo(IFileSystemItem item)
        {
            UnderlyingDirectory.CopyTo(item);
        }

        public void MoveTo(IFileSystemItem item)
        {
            UnderlyingDirectory.MoveTo(item);
        }


        ~TemporaryDirectory()
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