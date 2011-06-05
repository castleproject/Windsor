namespace Castle.IO.FileSystems.Local.Unix
{
    public class UnixFileSystem : LocalFileSystem
    {
        public override IDirectory GetDirectory(string directoryPath)
        {
            return new UnixDirectory(directoryPath);
        }

        public override IDirectory GetTempDirectory()
        {
            return new UnixDirectory(System.IO.Path.GetTempPath());
        }

    }
}