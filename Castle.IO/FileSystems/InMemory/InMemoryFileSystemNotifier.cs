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
using System.Linq;
using System.Threading;
using Castle.IO.Extensions;

namespace Castle.IO.FileSystems.InMemory
{
	public class InMemoryFileSystemNotifier : FileSystemNotifier
	{
		public override IFileSytemNotifierEntry CreateEntry(FileSystemNotificationIdentifier notificationIdentifier)
		{
			return new InMemoryFileSystemNotifierEntry(notificationIdentifier);
		}

		public void NotifyCreation(IFile file)
		{
			ThreadPool.QueueUserWorkItem(x =>
				{
					lock (Watchers)
					{
						foreach (var matching in from kv in Watchers
						                         let key = kv.Key
						                         let value = kv.Value as InMemoryFileSystemNotifierEntry
						                         where value != null &&
						                               (key.ChangeTypes & WatcherChangeTypes.Created) == WatcherChangeTypes.Created &&
						                               PathMatches(key, file) &&
						                               key.Filter.Wildcard().IsMatch(file.Name)
						                         select value)
							matching.ExecuteHandlers(file);
					}
				});
		}

		private static bool PathMatches(FileSystemNotificationIdentifier key, IFile file)
		{
			return key.IncludeSubDirectories
			       	? file.Parent.Path.FullPath.StartsWith(key.Path.FullPath, StringComparison.OrdinalIgnoreCase)
			       	: file.Parent.Path == key.Path;
		}
	}
}