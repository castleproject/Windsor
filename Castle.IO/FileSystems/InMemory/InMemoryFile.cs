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

namespace Castle.IO.FileSystems.InMemory
{
	public class InMemoryFile : IFile
	{
		private byte[] _content = new byte[4096];
		private long _contentLength;
		private readonly object _contentLock = new object();

		private void CopyFromFile(InMemoryFile fileToCopy)
		{
			Exists = true;
			_LastModifiedTimeUtc = fileToCopy.GetLastModifiedTimeUtc();
			lock (fileToCopy._contentLock)
			{
				var newContent = new byte[fileToCopy._content.Length];
				Buffer.BlockCopy(fileToCopy._content, 0, newContent, 0, fileToCopy._content.Length);
				_content = newContent;
				_contentLength = fileToCopy._contentLength;
			}
		}

		public InMemoryFile(string filePath)
		{
			Path = new Path(filePath);
			Name = Path.GetFileName(filePath);
			NameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

			_LastModifiedTimeUtc = null;
		}

		private FileShare? _lock;

		private void FileStreamClosed()
		{
			_lock = null;
		}

		public IFile Create()
		{
			EnsureExists();
			return this;
		}

		private void EnsureExists()
		{
			if (Exists) return;

			Exists = true;
			_LastModifiedTimeUtc = DateTimeOffset.UtcNow;

			if (Parent != null && !Parent.Exists)
				Parent.Create();

			if (Parent != null)
			{
				((InMemoryFileSystem) Parent.FileSystem).Notifier.NotifyCreation(this);
			}
		}

		public void CopyTo(IFileSystemItem where)
		{
			VerifyExists();
			var currentLock = _lock;
			if (currentLock != null && currentLock.Value != FileShare.Read && currentLock.Value != FileShare.ReadWrite)
				throw new IOException("Cannot copy file as someone opened it without shared read access");
			if (where is InMemoryFile)
			{
				if (where.Exists)
					throw new IOException("File already exists");
				((InMemoryFile) where).CopyFromFile(this);
			}
			else if (where is InMemoryDirectory)
				((InMemoryFile) ((InMemoryDirectory) where).GetFile(Name)).CopyFromFile(this);
			else
				throw new InvalidOperationException("The target type doesn't match the file system of the current file.");
		}

		public void MoveTo(IFileSystemItem newFileName)
		{
			var currentLock = _lock;
			if (currentLock != null) throw new IOException("File is locked, please try again later.");
			CopyTo(newFileName);
			Delete();
		}

		public Path Path { get; set; }

		public IDirectory Parent { get; set; }

		public IFileSystem FileSystem { get; set; }
		private bool _exists;

		public bool Exists
		{
			get { return _exists; }
			set { _exists = value; }
		}

		public string Name { get; private set; }

		public void Delete()
		{
			Exists = false;
		}

		public string NameWithoutExtension { get; private set; }

		public string Extension
		{
			get { return Path.GetExtension(Name); }
		}

		public long GetSize()
		{
			if (!Exists)
				throw new FileNotFoundException();

			return _contentLength;
		}

		private DateTimeOffset? _LastModifiedTimeUtc;

		public DateTimeOffset? GetLastModifiedTimeUtc()
		{
			return _LastModifiedTimeUtc;
		}

		public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			Validate.FileAccess(fileMode, fileAccess);

			ValidateFileMode(fileMode);

			var beginPosition = ValidateFileMode(fileMode);
			ValidateFileLock(fileAccess, fileShare);

			return new FileStreamDouble(new InMemFileStream(this, fileAccess)
				{
					Position = beginPosition
				}, FileStreamClosed);
		}

		private void VerifyExists()
		{
			if (Exists == false)
				throw new FileNotFoundException("File does not exist.", Path.ToString());
		}

		private void ValidateFileLock(FileAccess fileAccess, FileShare fileShare)
		{
			if (_lock == null)
			{
				_lock = fileShare;
				return;
			}

			var readAllowed = _lock.Value == FileShare.Read || _lock.Value == FileShare.ReadWrite;
			var writeAllowed = _lock.Value == FileShare.Write || _lock.Value == FileShare.ReadWrite;

			if ((fileAccess == FileAccess.Read && !readAllowed) ||
			    (fileAccess == FileAccess.ReadWrite && !(readAllowed && writeAllowed)) ||
			    (fileAccess == FileAccess.Write && !writeAllowed))
				throw new IOException("File is locked. Please try again.");
		}

		private long ValidateFileMode(FileMode fileMode)
		{
			if (Exists)
			{
				switch (fileMode)
				{
					case FileMode.CreateNew:
						throw new IOException("File already exists.");
					case FileMode.Create:
					case FileMode.Truncate:
						_contentLength = 0;
						return 0;
					case FileMode.Open:
					case FileMode.OpenOrCreate:
						return 0;
					case FileMode.Append:
						return _contentLength;
				}
			}
			else
			{
				switch (fileMode)
				{
					case FileMode.Create:
					case FileMode.Append:
					case FileMode.CreateNew:
					case FileMode.OpenOrCreate:
						EnsureExists();
						return _contentLength;
					case FileMode.Open:
					case FileMode.Truncate:
						throw new FileNotFoundException();
				}
			}
			throw new ArgumentException("fileMode not recognized.");
		}

		public override string ToString()
		{
			return Path.FullPath;
		}

		private class InMemFileStream : Stream
		{
			private readonly InMemoryFile _file;
			private readonly FileAccess _fileAccess;
			private long _position;

			public InMemFileStream(InMemoryFile file, FileAccess fileAccess)
			{
				_file = file;
				_fileAccess = fileAccess;
			}

			public override void Flush()
			{
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				if (origin == SeekOrigin.Begin)
				{
					return MoveTo(offset);
				}
				if (origin == SeekOrigin.Current)
				{
					return MoveTo(_position + offset);
				}
				if (origin == SeekOrigin.End)
				{
					long size;
					lock (_file._contentLock)
						size = _file._contentLength;
					return MoveTo(size - offset);
				}
				throw new ArgumentException("origin is not a recognized value");
			}

			private long MoveTo(long offset)
			{
				if (offset < 0)
					throw new ArgumentException("Cannot stream there");
				if (offset > _file._contentLength)
					_file.Resize(offset);
				_position = offset;
				return offset;
			}

			public override void SetLength(long value)
			{
				_file.Resize(value);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				var end = _position + count;
				var fileSize = _file._contentLength;
				var maxLengthToRead = end > fileSize ? fileSize - _position : count;
				Buffer.BlockCopy(_file._content, (int) _position, buffer, offset, (int) maxLengthToRead);
				_position += maxLengthToRead;
				return (int) maxLengthToRead;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				var fileSize = _file._contentLength;

				var endOfWrite = (_position + count);
				if (endOfWrite > fileSize)
					_file.Resize(endOfWrite);

				Buffer.BlockCopy(buffer, offset, _file._content, (int) _position, count);
				_position = _position + count;
			}

			public override bool CanRead
			{
				get { return _fileAccess == FileAccess.Read || _fileAccess == FileAccess.ReadWrite; }
			}

			public override bool CanSeek
			{
				get { return true; }
			}

			public override bool CanWrite
			{
				get { return _fileAccess == FileAccess.Write || _fileAccess == FileAccess.ReadWrite; }
			}

			public override long Length
			{
				get
				{
					lock (_file._contentLock)
						return _file._contentLength;
				}
			}

			public override long Position
			{
				get { return _position; }
				set { Seek(value, SeekOrigin.Begin); }
			}
		}

		private void Resize(long offset)
		{
			lock (_contentLock)
			{
				if (_contentLength < offset)
					_contentLength = offset;
				if (_content.Length < _contentLength)
				{
					var buffer = new byte[_content.Length*2];
					Buffer.BlockCopy(_content, 0, buffer, 0, _content.Length);
					_content = buffer;
				}
			}
		}
	}
}