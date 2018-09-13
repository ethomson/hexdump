using System;
using System.Collections.Generic;
using System.IO;

namespace HexDump
{
    public class ConcatenatedStream : Stream
    {
        private readonly Stream[] streams;
        private int currentStream = 0;

        private long totalLength = -1;

        public ConcatenatedStream(string[] filenames, FileMode mode, FileAccess access, FileShare share)
        {
            streams = new Stream[filenames.Length];

            for (int i = 0; i < filenames.Length; i++)
            {
                streams[i] = new FileStream(filenames[i], mode, access, share);
            }
        }

        public ConcatenatedStream(Stream[] streams)
        {
            this.streams = streams;
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
                if (totalLength < 0)
                {
                    long sum = 0;

                    foreach (Stream stream in streams)
                    {
                        sum += streams.Length;
                    }

                    totalLength = sum;
                }

                return totalLength;
            }
        }

        public override long Position
        {
            get
            {
                long pos = 0;

                for (int i = 0; i < currentStream; i++)
                {
                    pos += streams[i].Length;
                }

                pos += streams[currentStream].Position;

                return pos;
            }

            set
            {
                long pos = value;

                for (int i = 0; i < streams.Length; i++)
                {
                    if (pos <= streams[i].Length)
                    {
                        streams[i].Position = pos;
                        currentStream = i;
                        return;
                    }

                    pos -= streams[i].Length;
                }

                throw new IOException(String.Format("Position {0} is past the end of buffers", value));
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                Position += offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                Position = offset;
            }
            else if (origin == SeekOrigin.End)
            {
                Position -= offset;
            }

            return Position;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int tot = 0;
            int remain = count;

            while (remain > 0)
            {
                if (streams[currentStream].Position == streams[currentStream].Length)
                {
                    if (currentStream + 1 == streams.Length)
                    {
                        break;
                    }

                    currentStream++;
                }

                int len = streams[currentStream].Read(buffer, offset, remain);

                if (len == 0)
                {
                    throw new IOException("Couldn't read from stream {0}, but not at EOF?", currentStream);
                }

                tot += len;
                offset += len;
                remain -= len;
            }

            return tot;
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
            foreach (Stream stream in streams)
            {
                stream.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (Stream stream in streams)
            {
                stream.Dispose();
            }
        }
    }
}
