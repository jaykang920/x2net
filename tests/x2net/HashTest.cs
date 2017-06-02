using System;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class HashTest
    {
        [Fact]
        public void TestCreation()
        {
            // One-arg constructor with new
            Hash hash1 = new Hash(Hash.Seed);
            Assert.NotNull(hash1);
            Assert.Equal(hash1.Code, Hash.Seed);

            // Without new
            Hash hash2 = new Hash(Hash.Seed);
            Assert.NotNull(hash2);
        }

        [Fact]
        public void TestBool()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update(true);
            hash2.Update(true);
            hash3.Update(false);
            Assert.Equal(hash1.Code, hash2.Code);
            Assert.NotEqual(hash1.Code, hash3.Code);
        }

        [Fact]
        public void TestInt()
        {
            // Signed
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update(2);
            hash2.Update(2);
            hash3.Update(-2);
            Assert.Equal(hash1.Code, hash2.Code);
            Assert.NotEqual(hash1.Code, hash3.Code);

            // Unsigned
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            Hash hash6 = new Hash(Hash.Seed);
            hash4.Update((uint)2);
            hash5.Update((uint)2);
            hash6.Update(unchecked((uint)-2));
            Assert.Equal(hash4.Code, hash5.Code);
            Assert.NotEqual(hash4.Code, hash6.Code);

            Assert.Equal(hash1.Code, hash4.Code);
            Assert.Equal(hash3.Code, hash6.Code);
        }

        [Fact]
        public void TestLong()
        {
            // Signed
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update((long)2);
            hash2.Update((long)2);
            hash3.Update((long)-2);
            Assert.Equal(hash1.Code, hash2.Code);
            Assert.NotEqual(hash1.Code, hash3.Code);

            // Unsigned
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            Hash hash6 = new Hash(Hash.Seed);
            hash4.Update((ulong)2);
            hash5.Update((ulong)2);
            hash6.Update(unchecked((ulong)-2));
            Assert.Equal(hash4.Code, hash5.Code);
            Assert.NotEqual(hash4.Code, hash6.Code);

            Assert.Equal(hash1.Code, hash4.Code);
            Assert.Equal(hash3.Code, hash6.Code);
        }

        [Fact]
        public void TestFloat()
        {
            // Single-precision
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update(0.01f);
            hash2.Update(0.01f);
            hash3.Update(-0.01f);
            Assert.Equal(hash1.Code, hash2.Code);
            Assert.NotEqual(hash1.Code, hash3.Code);

            // Double-precision
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            Hash hash6 = new Hash(Hash.Seed);
            hash4.Update((double)0.01f);
            hash5.Update((double)0.01f);
            hash6.Update((double)-0.01f);
            Assert.Equal(hash4.Code, hash5.Code);
            Assert.NotEqual(hash4.Code, hash6.Code);

            Assert.Equal(hash1.Code, hash4.Code);
            Assert.Equal(hash3.Code, hash6.Code);
        }

        [Fact]
        public void TestString()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            hash1.Update("abcd");
            hash2.Update("abcd");
            hash3.Update("bcde");
            Assert.Equal(hash1.Code, hash2.Code);
            Assert.NotEqual(hash1.Code, hash3.Code);

            // Null reference handling
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            hash4.Update(0);
            hash5.Update((string)null);
            Assert.Equal(hash4.Code, hash5.Code);
        }

        [Fact]
        public void TestObject()
        {
            Hash hash1 = new Hash(Hash.Seed);
            Hash hash2 = new Hash(Hash.Seed);
            Hash hash3 = new Hash(Hash.Seed);
            Object obj1 = new Object();
            Object obj2 = obj1;
            int hashCode = obj1.GetHashCode();
            hash1.Update(hashCode);
            hash2.Update(obj1);
            hash3.Update(obj2);
            Assert.Equal(hash1.Code, hash2.Code);
            Assert.Equal(hash1.Code, hash3.Code);

            // Null reference handling
            Hash hash4 = new Hash(Hash.Seed);
            Hash hash5 = new Hash(Hash.Seed);
            hash4.Update(0);
            hash5.Update((object)null);
            Assert.Equal(hash4.Code, hash5.Code);
        }
    }
}
