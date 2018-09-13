using System.IO;
using Xunit;
using HexDump;

namespace HexDumpTests
{
    public class ForwardSeekingStreamTest
    {
        [Fact]
        public void CanFastForwardStreamFromStart()
        {
            var buffer = new byte[3];

            var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var forwardable = new ForwardSeekingStream(stream);

            forwardable.Seek(2, SeekOrigin.Begin);

            forwardable.Read(buffer, 0, 3);

            Assert.Equal(new byte[] { 3, 4, 5 }, buffer);
        }

        [Fact]
        public void CanFastForwardStreamAfterRead()
        {
            var buffer = new byte[2];

            var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var forwardable = new ForwardSeekingStream(stream);

            forwardable.Read(buffer, 0, 2);
            Assert.Equal(new byte[] { 1, 2 }, buffer);

            forwardable.Seek(1, SeekOrigin.Current);

            forwardable.Read(buffer, 0, 2);
            Assert.Equal(new byte[] { 4, 5 }, buffer);
        }
    }
}
