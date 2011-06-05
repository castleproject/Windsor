namespace Castle.IO.FileSystems.Local
{
    public class LocalFileSystemNotifier : FileSystemNotifier
    {
        public static FileSystemNotifier Instance
        {
            get;
            private set;
        }

        static LocalFileSystemNotifier()
        {
            Instance = new LocalFileSystemNotifier();
        }
        public override IFileSytemNotifierEntry CreateEntry(FileSystemNotificationIdentifier notificationIdentifier)
        {
            return new LocalFileSystemNotifierEntry(notificationIdentifier);
        }
    }
}