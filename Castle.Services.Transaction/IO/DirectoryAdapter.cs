using System;
using System.IO;
using Castle.Services.Transaction.IO;
using log4net;
using Path=Castle.Services.Transaction.IO.Path;

namespace Castle.Services.Transaction.IO
{
	/// <summary>
	/// Adapter which wraps the functionality in <see cref="File"/>
	/// together with native kernel transactions.
	/// </summary>
	public sealed class DirectoryAdapter : TxAdapterBase, IDirectoryAdapter
	{
		private readonly IMapPath _PathFinder;
		private readonly ILog _Logger = LogManager.GetLogger(typeof (DirectoryAdapter));

		///<summary>
		/// c'tor
		///</summary>
		///<param name="pathFinder"></param>
		///<param name="constrainToSpecifiedDir"></param>
		///<param name="specifiedDir"></param>
		public DirectoryAdapter(IMapPath pathFinder, bool constrainToSpecifiedDir, string specifiedDir)
			: base(constrainToSpecifiedDir, specifiedDir)
		{
			if (pathFinder == null) 
				throw new ArgumentNullException("pathFinder");

			_Logger.Debug("DirectoryAdapter created.");

			_PathFinder = pathFinder;
		}

		/// <summary>Creates a directory at the path given.
		/// Contrary to the Win32 API, doesn't throw if the directory already
		/// exists, but instead returns true. The 'safe' value to get returned 
		/// for be interopable with other path/dirutil implementations would
		/// hence be false (i.e. that the directory didn't already exist).
		/// </summary>
		///<param name="path">The path to create the directory at.</param>
		/// <remarks>True if the directory already existed, False otherwise.</remarks>
		public bool Create(string path)
		{
			AssertAllowed(path);

#if !MONO
			IFileTransaction tx;
			if (HasTransaction(out tx))
			{
				return ((IDirectoryAdapter)tx).Create(path);
			}
#endif
			if (Directory.Exists(path))
			{
				return true;
			}
			Directory.CreateDirectory(path);
			return false;
		}

		/// <summary>
		/// Checks whether the path exists.
		/// </summary>
		/// <param name="path">Path to check.</param>
		/// <returns>True if it exists, false otherwise.</returns>
		public bool Exists(string path)
		{
			AssertAllowed(path);
#if !MONO
			IFileTransaction tx;
			if (HasTransaction(out tx))
				return ((IDirectoryAdapter) tx).Exists(path);
#endif

			return Directory.Exists(path);
		}

		/// <summary>
		/// Deletes a folder recursively.
		/// </summary>
		/// <param name="path"></param>
		public void Delete(string path)
		{
			AssertAllowed(path);
#if !MONO
			IFileTransaction tx;
			if (HasTransaction(out tx))
			{
				((IDirectoryAdapter) tx).Delete(path);
				return;
			}
#endif
			Directory.Delete(path);
		}

		/// <summary>
		/// Deletes a folder.
		/// </summary>
		/// <param name="path">The path to the folder to delete.</param>
		/// <param name="recursively">
		/// Whether to delete recursively or not.
		/// When recursive, we delete all subfolders and files in the given
		/// directory as well.
		/// </param>
		public bool Delete(string path, bool recursively)
		{
			AssertAllowed(path);
#if !MONO
			IFileTransaction tx;
			if (HasTransaction(out tx))
			{
				return tx.Delete(path, recursively);
			}
#endif
			Directory.Delete(path, recursively);
			return true;
		}

		/// <summary>
		/// Gets the full path of the specified directory.
		/// </summary>
		/// <param name="path">The relative path.</param>
		/// <returns>A string with the full path.</returns>
		public string GetFullPath(string path)
		{
			AssertAllowed(path);
#if !MONO
			IFileTransaction tx;
			if (HasTransaction(out tx))
				return (tx).GetFullPath(path);
#endif
			return Path.GetFullPath(path);
		}

		///<summary>
		/// Gets the MapPath of the path. 
		/// 
		/// This will be relative to the root web directory if we're in a 
		/// web site and otherwise to the executing assembly.
		///</summary>
		///<param name="path"></param>
		///<returns></returns>
		public string MapPath(string path)
		{
			return _PathFinder.MapPath(path);
		}

		///<summary>
		/// TODO: Moves the directory from the original path to the new path.
		///</summary>
		///<param name="originalPath">Path from</param>
		///<param name="newPath">Path to</param>
		public void Move(string originalPath, string newPath)
		{
			AssertAllowed(originalPath);
			AssertAllowed(newPath);

			throw new NotImplementedException("This hasn't been completely implemented with the >255 character paths. Please help out and send a patch.");

//#if !MONO
//			IFileTransaction tx;
//			if (HasTransaction(out tx))
//			{
//				(tx as IDirectoryAdapter).Move(originalPath, newPath);
//				return;
//			}
//#endif
			

			//Directory.Move(originalPath, newPath);
		}
	}
}