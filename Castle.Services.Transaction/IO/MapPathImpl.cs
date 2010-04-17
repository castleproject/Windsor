using System;
using Castle.Services.Transaction.IO;

namespace Castle.Services.Transaction.IO
{
	///<summary>
	/// An implementation of the MapPath which seems to be working well with
	/// both testfixtures and online. Consumed by <see cref="IDirectoryAdapter"/>
	/// (or any other object wanting the functionality).
	///</summary>
	public class MapPathImpl : IMapPath
	{
		private readonly Func<string, string> _Function;

		///<summary>
		/// Default c'tor.
		///</summary>
		public MapPathImpl()
		{
		}

		/// <summary>
		/// Function may be null.
		/// </summary>
		/// <param name="function"></param>
		public MapPathImpl(Func<string, string> function)
		{
			_Function = function;
		}

		///<summary>
		/// Gets the absolute path given a string formatted
		/// as a map path, for example:
		/// "~/plugins" or "plugins/integrated" or "C:\a\b\c.txt" or "\\?\C:\a\b"
		/// would all be valid map paths.
		///</summary>
		///<param name="path"></param>
		///<returns></returns>
		public string MapPath(string path)
		{
			if (Path.IsRooted(path))
				return Path.GetFullPath(path);

			if (_Function != null)
				return _Function(path);

			path = Path.NormDirSepChars(path);

			if (path == string.Empty)
				return AppDomain.CurrentDomain.BaseDirectory;

			if (path[0] == '~')
				path = path.Substring(1);

			if (path == string.Empty)
				return AppDomain.CurrentDomain.BaseDirectory;

			if (Path.DirectorySeparatorChar == path[0])
				path = path.Substring(1);

			return path == string.Empty ? AppDomain.CurrentDomain.BaseDirectory : 
				Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory.Combine(path));
		}
	}
}