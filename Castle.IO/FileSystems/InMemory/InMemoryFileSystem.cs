using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.IO;

namespace OpenFileSystem.IO.FileSystems.InMemory
{
    
    public class InMemoryFileSystem : IFileSystem
    {
        readonly object _syncRoot = new object();
        public Dictionary<string, InMemoryDirectory> Directories { get; private set; }

        public InMemoryFileSystem()
        {
            Directories = new Dictionary<string, InMemoryDirectory>(StringComparer.OrdinalIgnoreCase);
            CurrentDirectory = @"c:\";
            Notifier = new InMemoryFileSystemNotifier();
        }

        public InMemoryFileSystemNotifier Notifier { get; set; }

        InMemoryDirectory GetRoot(string path)
        {
            InMemoryDirectory directory;
            lock (Directories)
            {
                if (!Directories.TryGetValue(path, out directory))
                {
                    Directories.Add(path, directory = new InMemoryDirectory(this, path));
                }
            }
            return directory;
        }
        public IDirectory GetDirectory(string directoryPath)
        {
            directoryPath = EnsureTerminatedByDirectorySeparator(directoryPath);
            var resolvedDirectoryPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(CurrentDirectory,directoryPath));
            var pathSegments = new Path(resolvedDirectoryPath).Segments;
            return pathSegments
                .Skip(1)
                .Aggregate((IDirectory)GetRoot(pathSegments.First()),
                    (current, segment) => current.GetDirectory(segment));
        }

        static string EnsureTerminatedByDirectorySeparator(string directoryPath)
        {
            return directoryPath.EndsWith(System.IO.Path.DirectorySeparatorChar + string.Empty)
                   || directoryPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar + string.Empty)
                       ? directoryPath
                       : directoryPath + System.IO.Path.DirectorySeparatorChar;
        }

        public IFile GetFile(string filePath)
        {
            var resolvedFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(CurrentDirectory, filePath));
            var pathSegments = new Path(resolvedFilePath).Segments;
            var ownerFolder = pathSegments
                .Skip(1).Take(pathSegments.Count()-2)
                .Aggregate((IDirectory)GetRoot(pathSegments.First()),
                    (current, segment) => current.GetDirectory(segment));
            return ownerFolder.GetFile(pathSegments.Last());
        }

        public Path GetPath(string path)
        {
            return new Path(path);
        }

        public ITemporaryDirectory CreateTempDirectory()
        {
            var sysTemp = (InMemoryDirectory)GetTempDirectory();

            var tempDirectory = new InMemoryTemporaryDirectory(this, sysTemp.Path.Combine(System.IO.Path.GetRandomFileName()).FullPath)
            {
                Exists = true,
                Parent = sysTemp
            };
            lock (sysTemp.ChildDirectories)
            {
                sysTemp.ChildDirectories.Add(tempDirectory);
            }
            return tempDirectory;
        }

        public IDirectory CreateDirectory(string path)
        {
            return GetDirectory(path).MustExist();
        }

        public ITemporaryFile CreateTempFile()
        {
            var tempDirectory = (InMemoryDirectory)GetTempDirectory();
            var tempFile = new InMemoryTemporaryFile(tempDirectory.Path.Combine(System.IO.Path.GetRandomFileName()).ToString())
            {
                Exists = true,
                FileSystem = this,
                Parent = tempDirectory
            };
            tempDirectory.Create();
            tempDirectory.ChildFiles.Add(tempFile);
            
            return tempFile;
        }

        IDirectory _systemTempDirectory;
        public IDirectory GetTempDirectory()
        {
            if(_systemTempDirectory == null)
            {
                lock(_syncRoot)
                {
                    Thread.MemoryBarrier();
                    if (_systemTempDirectory == null)
                        _systemTempDirectory = GetDirectory(System.IO.Path.GetTempPath());
                }
            }
            return _systemTempDirectory;
        }

        public IDirectory GetCurrentDirectory()
        {
            return GetDirectory(CurrentDirectory);
        }

        public string CurrentDirectory { get; set; }
    }
}