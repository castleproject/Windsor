namespace Castle.IO.FileSystems.InMemory
{
    public class InMemoryTemporaryFile : InMemoryFile,  ITemporaryFile
    {
        public InMemoryTemporaryFile(string path) : base(path)
        {
        }

        public void Dispose()
        {
            Delete();
        }
    }
}