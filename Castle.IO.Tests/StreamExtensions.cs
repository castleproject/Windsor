#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.IO;
using Castle.IO;
using OpenWrap.Testing;

namespace OpenFileSystem.Tests
{
	public static class StreamExtensions
	{
		public static void Write(this IFile file, Action<Stream> writer, FileMode mode = FileMode.Create,
		                         FileAccess access = FileAccess.Write, FileShare share = FileShare.None)
		{
			using (var stream = file.Open(mode, access, share))
				writer(stream);
		}

		public static void Write(this IFile file, byte data = (byte) 0, FileMode mode = FileMode.Create,
		                         FileAccess access = FileAccess.Write, FileShare share = FileShare.None)
		{
			using (var stream = file.Open(mode, access, share))
				stream.WriteByte(data);
		}

		public static IFile ShouldBe(this IFile file, params byte[] content)
		{
			using (var stream = file.OpenRead())
				stream.ReadToEnd().ShouldBe(content);
			return file;
		}

		public static byte[] ReadToEnd(this Stream stream)
		{
			var streamToReturn = stream as MemoryStream;
			if (streamToReturn == null)
			{
				streamToReturn = new MemoryStream();
				stream.CopyTo(streamToReturn);
				streamToReturn.Position = 0;
			}

			var destinationBytes = new byte[streamToReturn.Length - streamToReturn.Position];
			Buffer.BlockCopy(streamToReturn.GetBuffer(),
			                 (int) streamToReturn.Position,
			                 destinationBytes,
			                 0,
			                 (int) (streamToReturn.Length - streamToReturn.Position));
			return destinationBytes;
		}
	}
}