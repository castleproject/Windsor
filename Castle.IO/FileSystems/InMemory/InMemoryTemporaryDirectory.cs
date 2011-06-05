namespace Castle.IO.FileSystems.InMemory
{
    public class InMemoryTemporaryDirectory : InMemoryDirectory, ITemporaryDirectory
    {
        public InMemoryTemporaryDirectory(InMemoryFileSystem fs, string path) : base(fs, path)
        {
        }

        public void Dispose()
        {
            Delete();
        }
    }
}