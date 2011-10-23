// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.IO.FileSystems.Local
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;

	using Castle.IO.Internal;

	using Path = Castle.IO.Path;

	public class LocalFile : IFile, IEquatable<LocalFile>
	{
		private readonly string filePath;
		private readonly Func<Path, IDirectory> directoryFactory;

		public LocalFile(string filePath, Func<Path, IDirectory> directoryFactory)
		{
			Contract.Requires(filePath != null);
			Contract.Requires(directoryFactory != null);

			this.filePath = filePath;
			this.directoryFactory = directoryFactory;

			Path = new Path(filePath);
		}

		public virtual bool Exists
		{
			get
			{
				// TODO: override this for the windows file system to avoid too long path exceptions
				var i = Path.Info;
				var p = Path.FullPath;
				var file = p.StartsWith(i.UNCPrefix) ? p.Substring(i.UNCPrefix.Length).TrimStart('\\') : p;
				return File.Exists(file);
			}
		}

		public virtual IFileSystem FileSystem
		{
			get { return LocalFileSystem.Instance; }
		}

		public virtual string Name
		{
			get { return Path.GetFileName(filePath); }
		}

		public virtual string NameWithoutExtension
		{
			get { return Path.GetFileNameWithoutExtension(filePath); }
		}

		public virtual string Extension
		{
			get { return Path.GetExtension(filePath); }
		}

		public virtual long GetSize()
		{
			// TODO: use long path instead, to void short path limits
			return new FileInfo(filePath).Length;
		}

		public virtual DateTimeOffset? GetLastModifiedTimeUtc()
		{
			// TODO: use long path instead, to void short path limits
			return Exists ? new DateTimeOffset(new FileInfo(filePath).LastWriteTimeUtc, TimeSpan.Zero) : (DateTimeOffset?)null;
		}

		public override string ToString()
		{
			return Path.FullPath;
		}

		public virtual IDirectory Parent
		{
			get
			{
				try
				{
					var path = Path.GetPathWithoutLastBit(filePath);
					return path == null
					       	? null
					       	: directoryFactory(path);
				}
				catch (DirectoryNotFoundException)
				{
					return null;
				}
			}
		}

		public virtual Path Path { get; private set; }

		public virtual Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (!Exists && !Parent.Exists)
				if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.OpenOrCreate)
					Parent.Create();

			Validate.FileAccess(fileMode, fileAccess);

			return LongPathFile.Open(filePath, fileMode, fileAccess, fileShare);
		}

		public virtual void Delete()
		{
			LongPathFile.Delete(filePath);
		}

		public virtual void CopyTo(IFileSystemItem item)
		{
			string destinationPath;

			if (item is IDirectory)
			{
				((IDirectory)item).MustExist();
				destinationPath = item.Path.Combine(Name).FullPath;
			}
			else
			{
				item.Parent.MustExist();
				destinationPath = (item).Path.FullPath;
			}

			LongPathFile.Copy(filePath, destinationPath, true);
		}

		public virtual void MoveTo(IFileSystemItem item)
		{
			File.Move(filePath, item.Path.FullPath);
		}

		public virtual IFile Create()
		{
			// creates the parent if it doesnt exist
			if (!Parent.Exists)
				Parent.Create();

			File.Create(Path.FullPath).Close();
			return this;
		}

		#region equality implementation

		public bool Equals(LocalFile other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Path, Path);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(LocalFile)) return false;
			return Equals((LocalFile)obj);
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode();
		}

		public static bool operator ==(LocalFile left, LocalFile right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(LocalFile left, LocalFile right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}