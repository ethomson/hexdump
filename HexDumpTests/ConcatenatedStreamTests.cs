using System.IO;
using Xunit;
using HexDump;

namespace HexDumpTests
{
    public class ConcatenatedFileStreamTests
    {
        [Fact]
        public void CanCatenateSingleStream()
        {
            var buffer = new byte[5];

            var one = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var concatenated = new ConcatenatedStream(new Stream[] { one });

            concatenated.Read(buffer, 0, 5);

            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, buffer);
        }

        [Fact]
        public void CanConcatenateTwoStreams()
        {
            byte[] buffer = new byte[10];

            var one = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var two = new MemoryStream(new byte[] { 6, 7, 8, 9, 10 });
            var concatenated = new ConcatenatedStream(new Stream[] { one, two });

            concatenated.Read(buffer, 0, 10);

            Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, buffer);
        }

        [Fact]
        public void CanConcatenateThreeStreams()
        {
            byte[] buffer = new byte[15];

            var one = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var two = new MemoryStream(new byte[] { 6, 7, 8, 9, 10 });
            var three = new MemoryStream(new byte[] { 11, 12, 13, 14, 15 });
            var concatenated = new ConcatenatedStream(new Stream[] { one, two, three });

            concatenated.Read(buffer, 0, 15);

            Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, buffer);
        }
    }
}
