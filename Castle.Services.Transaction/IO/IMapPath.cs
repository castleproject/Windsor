namespace Castle.Services.Transaction.IO
{
	///<summary>
	/// Small interface for the map path functionality.
	///</summary>
	public interface IMapPath
	{
		///<summary>
		/// Gets the absolute path given a string formatted
		/// as a map path, for example:
		/// "~/plugins" or "plugins/integrated" or "C:\a\b\c.txt" or "\\?\C:\a\b"
		/// would all be valid map paths.
		///</summary>
		///<param name="path"></param>
		///<returns></returns>
		string MapPath(string path);
	}
}