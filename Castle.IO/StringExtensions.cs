using System.Text.RegularExpressions;

namespace OpenFileSystem.IO
{
    public static class StringExtensions
    {
        public static Regex Wildcard(this string stringValue)
        {
            stringValue = Regex.Escape(stringValue).Replace("\\?", ".?").Replace("\\*", ".*");
            return new Regex("^" + stringValue + "$");
        }
    }
}