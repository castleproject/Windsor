using System;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.IO;
using Castle.IO.FileSystems;

namespace OpenFileSystem.IO.FileSystems.InMemory
{
    public class InMemoryFileSystemNotifier : FileSystemNotifier
    {
        public override IFileSytemNotifierEntry CreateEntry(FileSystemNotificationIdentifier notificationIdentifier)
        {
            return new InMemoryFileSystemNotifierEntry(notificationIdentifier);
        }
        public void NotifyCreation(IFile file)
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                lock (Watchers)
                {
                    foreach (var matching in from kv in Watchers
                                             let key = kv.Key
                                             let value = kv.Value as InMemoryFileSystemNotifierEntry
                                             where value != null &&
                                                   (key.ChangeTypes & WatcherChangeTypes.Created) == WatcherChangeTypes.Created &&
                                                   PathMatches(key, file) &&
                                                   key.Filter.Wildcard().IsMatch(file.Name)
                                             select value)
                        matching.ExecuteHandlers(file);
                }
            });


        }

        static bool PathMatches(FileSystemNotificationIdentifier key, IFile file)
        {
            return key.IncludeSubDirectories 
                       ? file.Parent.Path.FullPath.StartsWith(key.Path.FullPath, StringComparison.OrdinalIgnoreCase)
                       : file.Parent.Path == key.Path;
        }
    }
}