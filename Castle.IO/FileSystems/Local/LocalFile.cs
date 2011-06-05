using System;
using System.IO;

namespace Castle.IO.FileSystems.Local
{
    public class LocalFile : IFile, IEquatable<LocalFile>
    {
        public bool Equals(LocalFile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Path, Path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(LocalFile)) return false;
            return Equals((LocalFile)obj);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(LocalFile left, LocalFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LocalFile left, LocalFile right)
        {
            return !Equals(left, right);
        }

        readonly string _filePath;
        readonly Func<DirectoryInfo, IDirectory> _directoryFactory;

        public LocalFile(string filePath, Func<DirectoryInfo, IDirectory> directoryFactory)
        {
            _filePath = filePath;
            _directoryFactory = directoryFactory;
            Path = new Path(filePath);
        }

        public bool Exists
        {
            get { return File.Exists(_filePath); }
        }

        public IFileSystem FileSystem
        {
            get { return LocalFileSystem.Instance; }
        }

        public string Name
        {
            get { return System.IO.Path.GetFileName(_filePath); }
        }

        public string NameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(_filePath); }
        }

        public string Extension
        {
            get { return System.IO.Path.GetExtension(_filePath); }
        }

        public long Size
        {
            get { return new FileInfo(_filePath).Length; }
        }

        public DateTimeOffset? LastModifiedTimeUtc
        {
            get { return Exists ? new DateTimeOffset(new FileInfo(_filePath).LastWriteTimeUtc, TimeSpan.Zero) : (DateTimeOffset?)null; }
            set { if (value != null) new FileInfo(_filePath).LastWriteTimeUtc = value.Value.UtcDateTime; }
        }

        public override string ToString()
        {
            return Path.FullPath;
        }
        public IDirectory Parent
        {
            get
            {
                try
                {
                    var directoryInfo = Directory.GetParent(_filePath);
                    return directoryInfo == null
                               ? null
                               : _directoryFactory(directoryInfo);
                }
                catch (DirectoryNotFoundException)
                {
                    return null;
                }
            }
        }

        public Path Path { get; private set; }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            if (!Exists && !Parent.Exists)
            {
                if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.OpenOrCreate)
                    Parent.Create();
            }
            return File.Open(_filePath, fileMode, fileAccess, fileShare);
        }

        public void Delete()
        {
            File.Delete(_filePath);
        }

        public void CopyTo(IFileSystemItem item)
        {
            string destinationPath;

            if (item is IDirectory)
            {
                ((IDirectory)item).MustExist();
                destinationPath = item.Path.Combine(Name).FullPath;
            }
            else
            {
                item.Parent.MustExist();
                destinationPath = ((IFile)item).Path.FullPath;
            }
            
            
            File.Copy(_filePath, destinationPath);
        }

        public void MoveTo(IFileSystemItem item)
        {
            File.Move(_filePath, item.Path.FullPath);
        }

        public IFile Create()
        {
            // creates the parent if it doesnt exist
            if (!Parent.Exists)
                Parent.Create();

            File.Create(Path.FullPath).Close();
            return this;
        }
    }
}