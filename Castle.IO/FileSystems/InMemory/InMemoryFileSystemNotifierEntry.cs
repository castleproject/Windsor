using System;
using System.Collections.Generic;
using System.Linq;
using Castle.IO.Internal;

namespace Castle.IO.FileSystems.InMemory
{
    public class InMemoryFileSystemNotifierEntry : IFileSytemNotifierEntry
    {
        public FileSystemNotificationIdentifier Identifier { get; private set; }
        readonly List<Action<IFile>> _handlers = new List<Action<IFile>>();

        public InMemoryFileSystemNotifierEntry(FileSystemNotificationIdentifier notificationIdentifier)
        {
            Identifier = notificationIdentifier;
        }

        public IDisposable AddNotifiers(params Action<IFile>[] entries)
        {
            lock(_handlers)
            {
                _handlers.AddRange(entries.Where(x=>x != null));
            }
            return new ExecuteOnDispose(()=>
            {
                lock(_handlers)
                {
                    foreach(var value in entries)
                        _handlers.Remove(value);
                }
            });
        }

        public void ExecuteHandlers(IFile file)
        {
            IEnumerable<Action<IFile>> handlers;
            lock (_handlers)
            {
                handlers = _handlers.Copy();
            }
            foreach(var handler in handlers)
                handler(file);
        }
    }
}