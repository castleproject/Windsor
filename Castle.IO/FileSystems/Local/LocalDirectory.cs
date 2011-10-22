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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Castle.IO.FileSystems.Local
{
	using System.Diagnostics.Contracts;

	using Castle.IO.Extensions;

	public abstract class LocalDirectory : AbstractDirectory, IDirectory, IEquatable<IDirectory>
	{
		private DirectoryInfo DirectoryInfo { get; set; }

		protected LocalDirectory(DirectoryInfo directory)
		{
			Contract.Requires(directory != null);
			Contract.Ensures(Path != null);

			DirectoryInfo = directory;
			Path = new Path(NormalizeDirectoryPath(DirectoryInfo.FullName));
		}

		protected LocalDirectory(string directoryPath)
			: this(new DirectoryInfo(directoryPath))
		{
			Contract.Requires(directoryPath != null);
		}

		protected abstract LocalDirectory CreateDirectory(DirectoryInfo di);
		protected abstract LocalDirectory CreateDirectory(Path path);

		public bool Exists
		{
			get
			{
				DirectoryInfo.Refresh();
				return DirectoryInfo.Exists;
			}
		}

		public override string ToString()
		{
			return Path.FullPath;
		}

		public IFileSystem FileSystem
		{
			get { return LocalFileSystem.Instance; }
		}

		public string Name
		{
			get { return DirectoryInfo.Name; }
		}

		public virtual IDirectory Parent
		{
			get { return DirectoryInfo.Parent == null ? null : CreateDirectory(DirectoryInfo.Parent); }
		}

		public Path Path { get; private set; }

		public virtual bool IsHardLink
		{
			get { return false; }
		}

		public abstract IDirectory LinkTo(Path path);

		public virtual IDirectory Target
		{
			get { return this; }
		}

		public IDisposable FileChanges(string filter = "*", bool includeSubdirectories = false, Action<IFile> created = null,
		                               Action<IFile> modified = null, Action<IFile> deleted = null,
		                               Action<IFile> renamed = null)
		{
			return LocalFileSystemNotifier.Instance.RegisterNotification(Path, filter, includeSubdirectories, created, modified,
			                                                             deleted, renamed);
		}

		public virtual IEnumerable<IDirectory> Directories(string filter, SearchScope scope)
		{
			filter = filter.Trim();

			if (filter.EndsWith(Path.DirectorySeparatorChar + "") || filter.EndsWith(Path.AltDirectorySeparatorChar + ""))
				filter = filter.Substring(0, filter.Length - 1);

			DirectoryInfo.Refresh();

			if (Path.IsPathRooted(filter) || Regex.IsMatch(filter, "[A-Z]{1,3}:", RegexOptions.IgnoreCase))
			{
				var root = Path.GetPathRoot(filter);
				filter = filter.Substring(root.Length);
				return LocalFileSystem.Instance.GetDirectory(root).Directories(filter, scope);
			}

			return DirectoryInfo.GetDirectories(filter,
			                                    scope == SearchScope.CurrentOnly
			                                    	? SearchOption.TopDirectoryOnly
			                                    	: SearchOption.AllDirectories)
				.Select(x => (IDirectory) CreateDirectory(x));
		}

		public IEnumerable<IFile> Files()
		{
			DirectoryInfo.Refresh();
			return DirectoryInfo.GetFiles().Select(x => (IFile) new LocalFile(x.FullName, CreateDirectory));
		}

		public IEnumerable<IFile> Files(string filter, SearchScope scope)
		{
			filter = filter.Trim();
			if (filter.EndsWith(Path.DirectorySeparatorChar + "") ||
			    filter.EndsWith(Path.AltDirectorySeparatorChar + ""))
				filter += Path.DirectorySeparatorChar + "*";

			DirectoryInfo.Refresh();
			return
				DirectoryInfo.GetFiles(filter,
				                       scope == SearchScope.CurrentOnly
				                       	? SearchOption.TopDirectoryOnly
				                       	: SearchOption.AllDirectories)
					.Select(x => new LocalFile(x.FullName, CreateDirectory))
					.Cast<IFile>();
		}

		public virtual IDirectory GetDirectory(string directoryName)
		{
			return CreateDirectory(new Path(DirectoryInfo.FullName.Combine(directoryName)));
		}

		public IFile GetFile(string fileName)
		{
			return new LocalFile(System.IO.Path.Combine(DirectoryInfo.FullName, fileName), CreateDirectory);
		}

		public virtual IEnumerable<IDirectory> Directories()
		{
			DirectoryInfo.Refresh();
			return DirectoryInfo.GetDirectories().Select(x => (IDirectory) CreateDirectory(x));
		}

		public virtual void Delete()
		{
			DirectoryInfo.Refresh();
			if (DirectoryInfo.Exists)
				DirectoryInfo.Delete(true);
		}

		public virtual IDirectory Create()
		{
			DirectoryInfo.Refresh();
			DirectoryInfo.Create();
			return this;
		}

		public void MoveTo(IFileSystemItem newFileName)
		{
			var oldPath = Path.FullPath;
			DirectoryInfo.Refresh();
			DirectoryInfo.MoveTo(newFileName.Path.FullPath);
			DirectoryInfo = new DirectoryInfo(oldPath);
		}

		public void CopyTo(IFileSystemItem newItem)
		{
			var destDir = (IDirectory) newItem;

			DirectoryInfo.Refresh();
			if (!DirectoryInfo.Exists)
				DirectoryInfo.Create();
			destDir.MustExist();
			foreach (var file in Files())
				file.CopyTo(newItem);
			foreach (var directory in Directories())
				directory.CopyTo(destDir.GetDirectory(directory.Name).MustExist());
			DirectoryInfo.Refresh();
		}

		public bool Equals(IDirectory other)
		{
			if (ReferenceEquals(null, other)) return false;
			return other.Path.Equals(Path);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj as IDirectory == null) return false;
			return Equals((IDirectory) obj);
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode();
		}
	}
}