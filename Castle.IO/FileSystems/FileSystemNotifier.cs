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

namespace Castle.IO.FileSystems
{
	public abstract class FileSystemNotifier : IFileSystemNotifier
	{
		private readonly IDictionary<FileSystemNotificationIdentifier, IFileSytemNotifierEntry> _watchers =
			new Dictionary<FileSystemNotificationIdentifier, IFileSytemNotifierEntry>();

		protected IDictionary<FileSystemNotificationIdentifier, IFileSytemNotifierEntry> Watchers
		{
			get { return _watchers; }
		}

		public abstract IFileSytemNotifierEntry CreateEntry(FileSystemNotificationIdentifier notificationIdentifier);

		public IDisposable RegisterNotification(Path path, string filter = "*", bool includeSubdirectories = false,
		                                        Action<IFile> created = null, Action<IFile> modified = null,
		                                        Action<IFile> deleted = null, Action<IFile> renamed = null)
		{
			if (created == null && modified == null && renamed == null && deleted == null)
				throw new ArgumentException("One event type (created, modified, renamed or deleted) must be specified.");

			WatcherChangeTypes watcher = 0;
			if (created != null) watcher |= WatcherChangeTypes.Created;
			if (modified != null) watcher |= WatcherChangeTypes.Changed;
			if (renamed != null) watcher |= WatcherChangeTypes.Renamed;
			if (deleted != null) watcher |= WatcherChangeTypes.Deleted;


			var id = new FileSystemNotificationIdentifier(path ?? new Path(Environment.CurrentDirectory), watcher, filter ?? "*",
			                                              includeSubdirectories);
			IFileSytemNotifierEntry entry = null;

			if (!Watchers.TryGetValue(id, out entry))
			{
				lock (Watchers)
				{
					if (!Watchers.TryGetValue(id, out entry))
						Watchers.Add(id, entry = CreateEntry(id));
				}
			}
			return entry.AddNotifiers(created, deleted, modified, renamed);
		}
	}
}