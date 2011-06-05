namespace Castle.IO.FileSystems.Local.Win32
{
    public class Win32FileSystem : LocalFileSystem
    {
        public override IDirectory GetDirectory(string directoryPath)
        {
            return new Win32Directory(directoryPath);
        }

        public override IDirectory GetTempDirectory()
        {
            return new Win32Directory(System.IO.Path.GetTempPath());
        }
        
    }
}
