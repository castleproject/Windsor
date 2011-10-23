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

namespace Castle.IO.FileSystems
{
	public class FileStreamDouble : Stream
	{
		private readonly Action _onClose;

		public FileStreamDouble(Stream innerStream, Action onClose)
		{
			_onClose = onClose;
			InnerStream = innerStream;
			_canRead = innerStream.CanRead;
			_canWrite = innerStream.CanWrite;
			_canSeek = innerStream.CanSeek;
		}

		public Stream InnerStream { get; private set; }

		public override void Close()
		{
			_onClose();
			InnerStream.Position = 0;
		}

		protected override void Dispose(bool disposing)
		{
			_onClose();
			InnerStream.Position = 0;
		}

		public override void Flush()
		{
			InnerStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return InnerStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			InnerStream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return InnerStream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			InnerStream.Write(buffer, offset, count);
		}

		public FileStreamDouble AsRead()
		{
			_canRead = true;
			_canWrite = false;
			return this;
		}

		public FileStreamDouble AsWrite()
		{
			_canRead = false;
			_canWrite = false;
			return this;
		}

		public FileStreamDouble AsReadWrite()
		{
			_canRead = true;
			_canWrite = true;
			return this;
		}

		private bool _canRead;
		private bool _canWrite;
		private readonly bool _canSeek;

		public override bool CanRead
		{
			get { return _canRead; }
		}

		public override bool CanSeek
		{
			get { return _canSeek; }
		}

		public override bool CanWrite
		{
			get { return _canWrite; }
		}

		public override long Length
		{
			get { return InnerStream.Length; }
		}

		public override long Position
		{
			get { return InnerStream.Position; }
			set { InnerStream.Position = value; }
		}
	}
}