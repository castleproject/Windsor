using System;
using Castle.IO;

namespace OpenFileSystem.IO.FileSystems.InMemory
{
    public class ChildItem
    {
        IDirectory _parentDir;

        public ChildItem(IDirectory parentDir)
        {
            _parentDir = parentDir;
        }

        public void CreateChildDir(string name, params Action<ChildItem>[] children)
        {
            var newParent = new ChildItem(_parentDir.GetDirectory(name).MustExist());
            foreach (var val in children)
                val(newParent);
        }
        public void CreateChildFile(string name)
        {
            _parentDir.GetFile(name).MustExist();
        }
    }
}