namespace Castle.IO
{
    public interface IFileSystemItem<T> : IFileSystemItem
        where T : IFileSystemItem
    {
        T Create();
    }

    public interface IFileSystemItem
    {
        Path Path { get; }
        IDirectory Parent { get; }
        IFileSystem FileSystem { get; }
        bool Exists { get; }
        string Name { get; }
        void Delete();
        void CopyTo(IFileSystemItem item);
        void MoveTo(IFileSystemItem item);
    }
}