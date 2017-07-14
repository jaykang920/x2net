using System;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class BufferTest
    {
        [Fact]
        public void TestBufferCreation()
        {
            var buf = new x2net.Buffer();

            Assert.True(IsPowerOfTwo(buf.BlockSize));
            Assert.True(buf.IsEmpty);
            Assert.True(buf.Length == 0);
            Assert.True(buf.Capacity > 0);
            Assert.True(buf.Position == 0);
        }

        private static bool IsPowerOfTwo(int x)
        {
            return x >= 1 && ((x & (~x + 1)) == x);
        }

        [Fact]
        public void TestBufferBasicPerformance()
        {
            // CopyTo 
            
            //  
        }

        [Fact]
        public void TestTrim()
        {
            var buf = new x2net.Buffer();

            buf.EnsureCapacityToWrite(buf.BlockSize);  // numBlocks + 1

            buf.Position = 24;

            // front <= pos
            buf.Trim();

            Assert.Equal(buf.BlockSize - 24, buf.Length);
            Assert.Equal(0, buf.Position);

            buf.Position = (int)buf.Length;

            // reset
            buf.Trim();

            Assert.Equal(0, buf.Length);
            Assert.Equal(0, buf.Position);

            buf.EnsureCapacityToWrite(buf.BlockSize);

            buf.MarkToRead(1024);
            buf.Position = 24;

            // front <= marker
            buf.Trim();

            Assert.Equal(buf.BlockSize - 1024, buf.Length);
            Assert.Equal(0, buf.Position);
        }
    }
}
