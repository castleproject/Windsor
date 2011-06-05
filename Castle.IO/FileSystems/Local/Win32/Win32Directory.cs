using System.IO;
using Castle.IO.FileSystems.Local.Win32.Interop;

namespace Castle.IO.FileSystems.Local.Win32
{
    public class Win32Directory : LocalDirectory
    {
        public Win32Directory(DirectoryInfo directory) : base(directory)
        {
            
        }
        public Win32Directory(string directoryPath) : base(directoryPath)
        {
            
        }
        public override void Delete()
        {
            if (IsHardLink)
                JunctionPoint.Delete(Path.FullPath);
            else
                base.Delete();
        }
        public override bool IsHardLink
        {
            get { return JunctionPoint.Exists(this.Path.FullPath); }
        }

        public override IDirectory LinkTo(string path)
        {
            path = NormalizeDirectoryPath(path);
            JunctionPoint.Create(path, this.Path.FullPath,true);
            return new Win32Directory(path);
        }
        protected override LocalDirectory CreateDirectory(DirectoryInfo di)
        {
            return new Win32Directory(di);
        }
        protected override LocalDirectory CreateDirectory(string path)
        {
            return new Win32Directory(path);
        }
        public override IDirectory Target
        {
            get { return IsHardLink ? new Win32Directory(JunctionPoint.GetTarget(this.Path.FullPath)) : this; }
        }
    }
}