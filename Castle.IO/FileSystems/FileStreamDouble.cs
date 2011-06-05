using System;
using System.IO;

namespace Castle.IO.FileSystems
{
    public class FileStreamDouble : Stream
    {
        readonly Action _onClose;

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
        bool _canRead;
        bool _canWrite;
        bool _canSeek;
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