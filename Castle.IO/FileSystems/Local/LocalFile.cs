#region license

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

#endregion

using System;
using System.IO;
using Castle.IO.Internal;
using Directory = System.IO.Directory;

namespace Castle.IO.FileSystems.Local
{
	public class LocalFile : IFile, IEquatable<LocalFile>
	{
		private readonly string _filePath;
		private readonly Func<DirectoryInfo, IDirectory> _directoryFactory;

		public LocalFile(string filePath, Func<DirectoryInfo, IDirectory> directoryFactory)
		{
			_filePath = filePath;
			_directoryFactory = directoryFactory;
			Path = new Path(filePath);
		}

		public virtual bool Exists
		{
			get { return File.Exists(_filePath); }
		}

		public virtual IFileSystem FileSystem
		{
			get { return LocalFileSystem.Instance; }
		}

		public virtual string Name
		{
			get { return Path.GetFileName(_filePath); }
		}

		public virtual string NameWithoutExtension
		{
			get { return Path.GetFileNameWithoutExtension(_filePath); }
		}

		public virtual string Extension
		{
			get { return Path.GetExtension(_filePath); }
		}

		public virtual long GetSize()
		{
			// TODO: use long path instead, to void short path limits
			return new FileInfo(_filePath).Length;
		}

		public virtual DateTimeOffset? GetLastModifiedTimeUtc()
		{
			// TODO: use long path instead, to void short path limits
			return Exists ? new DateTimeOffset(new FileInfo(_filePath).LastWriteTimeUtc, TimeSpan.Zero) : (DateTimeOffset?) null;
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
					var directoryInfo = Path.GetPathWithoutLastBit(_filePath);
					return directoryInfo == null
					       	? null
					       	: _directoryFactory(new DirectoryInfo(directoryInfo.FullPath));
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
			{
				if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew || fileMode == FileMode.OpenOrCreate)
					Parent.Create();
			}

			if ((fileAccess & FileAccess.Read) != 0 && fileMode == FileMode.Append)
				throw new ArgumentException("Cannot open file in read-mode when having FileMode.Append");

			return LongPathFile.Open(_filePath, fileMode, fileAccess, fileShare);
		}

		public virtual void Delete()
		{
			LongPathFile.Delete(_filePath);
		}

		public virtual void CopyTo(IFileSystemItem item)
		{
			string destinationPath;

			if (item is IDirectory)
			{
				((IDirectory) item).MustExist();
				destinationPath = item.Path.Combine(Name).FullPath;
			}
			else
			{
				item.Parent.MustExist();
				destinationPath = (item).Path.FullPath;
			}


			LongPathFile.Copy(_filePath, destinationPath, true);
		}

		public virtual void MoveTo(IFileSystemItem item)
		{
			File.Move(_filePath, item.Path.FullPath);
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
			if (obj.GetType() != typeof (LocalFile)) return false;
			return Equals((LocalFile) obj);
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