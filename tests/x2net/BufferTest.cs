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
    }
}
