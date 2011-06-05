using System;

namespace Castle.IO.FileSystems
{
    public interface IFileSytemNotifierEntry
    {
        IDisposable AddNotifiers(params Action<IFile>[] entries);
    }
}