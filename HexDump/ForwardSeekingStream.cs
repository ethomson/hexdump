using System;
using System.Collections.Generic;
using System.IO;

namespace HexDump
{
    public class ForwardSeekingStream : Stream
    {
        private Stream stream;
        private long position = 0;

        public ForwardSeekingStream(Stream stream)
        {
            this.stream = stream;
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                return position;
            }

            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                if (offset < position)
                {
                    throw new IOException("Cannot rewind stream");
                }

                FastForward(offset - position);
            }
            else if (origin == SeekOrigin.Current)
            {
                FastForward(offset);
            }
            else if (origin == SeekOrigin.End)
            {
                throw new IOException("Cannot rewind stream");
            }

            return position;
        }

        private bool FastForward(long count)
        {
            byte[] buf = new byte[1024];

            while (count > 0)
            {
                int ret = stream.Read(buf, 0, Math.Min((int)count, 1024));

                if (ret == 0)
                {
                    return false;
                }

                count -= ret;
                position += ret;
            }

            return true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int ret = stream.Read(buffer, offset, count);

            if (ret > 0)
            {
                position += ret;
            }

            return ret;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            stream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            stream.Dispose();
        }
    }
}
