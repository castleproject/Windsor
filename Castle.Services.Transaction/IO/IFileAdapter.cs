using System.IO;
using System.Text;

namespace Castle.Services.Transaction.IO
{
	///<summary>
	/// File helper wrapper interface.
	///</summary>
	public interface IFileAdapter
	{
		///<summary>
		/// Create a new file transactionally.
		///</summary>
		///<param name="filePath">The path, where to create the file.</param>
		///<returns>A handle pointing to the file.</returns>
		FileStream Create(string filePath);

		///<summary>
		/// Returns whether the specified file exists or not.
		///</summary>
		///<param name="filePath">The file path.</param>
		///<returns></returns>
		bool Exists(string filePath);

		/// <summary>
		/// Reads all text from a file as part of a transaction
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		string ReadAllText(string path);

		/// <summary>
		/// Writes text to a file as part of a transaction
		/// </summary>
		/// <param name="path"></param>
		/// <param name="contents"></param>
		void WriteAllText(string path, string contents);

		/// <summary>
		/// Deletes a file as part of a transaction
		/// </summary>
		/// <param name="filePath"></param>
		void Delete(string filePath);

		/// <summary>
		/// Opens a file with RW access.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="mode">The file mode, which specifies </param>
		/// <returns></returns>
		FileStream Open(string filePath, FileMode mode);

		/// <summary>
		/// Writes an input stream to the file path.
		/// </summary>
		/// <param name="toFilePath">The path to write to.</param>
		/// <param name="fromStream">The stream to read from.</param>
		/// <returns>The number of bytes written.</returns>
		int WriteStream(string toFilePath, Stream fromStream);

		///<summary>
		/// Reads all text in a file and returns the string of it.
		///</summary>
		///<param name="path"></param>
		///<param name="encoding"></param>
		///<returns></returns>
		string ReadAllText(string path, Encoding encoding);

		///<summary>
		/// Moves a file from one path to another.
		/// 
		/// These should all be equivalent:
		/// <code>
		/// Move("b/a.txt", "c/a.txt")
		/// Move("b/a.txt", "c") // given c either is a directory or doesn't exist, otherwise it overwrites the file c
		/// Move("b/a.txt", "c/") // c must be a directory and might or might not exist. If it doesn't exist it will be created.
		/// </code>
		///</summary>
		///<param name="originalFilePath">
		/// The original file path. It can't be null nor can it point to a directory.
		/// </param>
		/// ///<param name="newFilePath">The new location of the file.</param>
		void Move(string originalFilePath, string newFilePath);
	}
}