using System;
using System.IO;

namespace Castle.IO.FileSystems
{
    public class FileSystemNotificationIdentifier : IEquatable<FileSystemNotificationIdentifier>
    {
        public FileSystemNotificationIdentifier(Path path, WatcherChangeTypes changeTypes, string filter, bool includeSubDirectories)
        {
            IncludeSubDirectories = includeSubDirectories;
            Path = path;
            Filter = filter;
            ChangeTypes = changeTypes;
        }

        public bool IncludeSubDirectories { get; private set; }

        public bool Equals(FileSystemNotificationIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.IncludeSubDirectories.Equals(IncludeSubDirectories) && Equals(other.Path, Path) && Equals(other.Filter, Filter) && Equals(other.ChangeTypes, ChangeTypes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(FileSystemNotificationIdentifier)) return false;
            return Equals((FileSystemNotificationIdentifier)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = IncludeSubDirectories.GetHashCode();
                result = (result * 397) ^ Path.GetHashCode();
                result = (result * 397) ^ Filter.GetHashCode();
                result = (result * 397) ^ ChangeTypes.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(FileSystemNotificationIdentifier left, FileSystemNotificationIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileSystemNotificationIdentifier left, FileSystemNotificationIdentifier right)
        {
            return !Equals(left, right);
        }

        public Path Path { get; private set; }
        public string Filter { get; private set; }
        public WatcherChangeTypes ChangeTypes { get; private set; }
    }
}