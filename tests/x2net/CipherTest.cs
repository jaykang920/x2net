using System;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class CipherTest
    {
        [Fact]
        public void TestTransform()
        {
            BlockCipher cipher1 = new BlockCipher();
            BlockCipher cipher2 = new BlockCipher();

            byte[] bytes1 = cipher1.InitializeHandshake();
            byte[] bytes2 = cipher2.InitializeHandshake();
            bytes1 = cipher2.Handshake(bytes1);
            bytes2 = cipher1.Handshake(bytes2);
            cipher1.FinalizeHandshake(bytes1);
            cipher2.FinalizeHandshake(bytes2);

            var buffer = new x2net.Buffer();

            string text = new String('x', 5300);
            Assert.Equal(5300, text.Length);
            /*
            buffer.Write(1);
            buffer.Write(text);
            buffer.Shrink(1);
            */

            cipher1.Transform(buffer, (int)buffer.Length);
            cipher2.InverseTransform(buffer, (int)buffer.Length);

            buffer.Rewind();

            /*
            string result;
            buffer.Read(out result);

            Assert.AreEqual(text, result);
            */
        }
    }
}
