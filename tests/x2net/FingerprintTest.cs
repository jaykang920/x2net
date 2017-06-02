using System;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class FingerprintTest
    {
        [Fact]
        public void TestNegativeLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { var fp = new Fingerprint(-1); });
        }

        [Fact]
        public void TestAccessors()
        {
            var fp = new Fingerprint(33);

            Assert.Throws(typeof(IndexOutOfRangeException),
                () => { fp.Get(-1); });
            Assert.Throws(typeof(IndexOutOfRangeException),
                () => { fp.Get(33); });

            Assert.False(fp.Get(31));
            fp.Touch(31);
            Assert.True(fp.Get(31));
            fp.Wipe(31);
            Assert.False(fp.Get(31));

            Assert.False(fp.Get(32));
            fp.Touch(32);
            Assert.True(fp.Get(32));
            fp.Wipe(32);
            Assert.False(fp.Get(32));
        }

        [Fact]
        public void TestCreation()
        {
            var fp1 = new Fingerprint(1);
            Assert.Equal(1, fp1.Length);
            Assert.False(fp1.Get(0));
            
            Fingerprint fp2 = new Fingerprint(33);
            Assert.Equal(33, fp2.Length);
            for (int i = 0; i < 33; ++i)
            {
                Assert.False(fp2.Get(i));
            }
        }

        [Fact]
        public void TestGetLength()
        {
            var fp1 = new Fingerprint(99);

            Assert.Equal(99, fp1.Length);

            var blen = fp1.GetLength();

            var lengthInBytes = (((fp1.Length - 1) >> 3) + 1);
            var expectedBytesLen = lengthInBytes  + Serializer.GetLengthVariableNonnegative(99);

            Assert.Equal(expectedBytesLen, blen);
        }

        [Fact]
        public void TestCopyCreation()
        {
            Fingerprint fp1 = new Fingerprint(65);
            fp1.Touch(32);
            Fingerprint fp2 = new Fingerprint(fp1);
            Assert.True(fp2.Get(32));

            // Ensure that the original block array is not shared
            fp1.Touch(64);
            Assert.False(fp2.Get(64));
        }

        [Fact]
        public void TestClear()
        {
            // Length > 32
            Fingerprint fp = new Fingerprint(65);
            for (int i = 0; i < 65; ++i)
            {
                fp.Touch(i);
                Assert.True(fp.Get(i));
            }
            fp.Clear();
            for (int i = 0; i < 65; ++i)
            {
                Assert.False(fp.Get(i));
            }

            // Length <= 32
            var fp2 = new Fingerprint(6);
            for (int i = 0; i < 6; ++i)
            {
                fp2.Touch(i);
                Assert.True(fp2.Get(i));
            }
            fp.Clear();
            for (int i = 0; i < 6; ++i)
            {
                Assert.False(fp.Get(i));
            }
        }

        [Fact]
        public void TestComparison()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            // Length first

            Assert.True(fp3.CompareTo(fp1) < 0);
            Assert.True(fp1.CompareTo(fp3) > 0);
            fp3.Touch(2);
            Assert.True(fp3.CompareTo(fp1) < 0);
            Assert.True(fp1.CompareTo(fp3) > 0);

            Assert.True(fp4.CompareTo(fp2) > 0);
            Assert.True(fp2.CompareTo(fp4) < 0);
            fp2.Touch(64);
            Assert.True(fp4.CompareTo(fp2) > 0);
            Assert.True(fp2.CompareTo(fp4) < 0);
            fp2.Wipe(64);

            // Bits second
            Assert.Equal(0, fp1.CompareTo(fp2));

            fp1.Touch(31);
            Assert.True(fp1.CompareTo(fp2) > 0);
            Assert.True(fp2.CompareTo(fp1) < 0);

            fp2.Touch(32);
            Assert.True(fp1.CompareTo(fp2) < 0);
            Assert.True(fp2.CompareTo(fp1) > 0);

            fp1.Touch(32);
            Assert.True(fp1.CompareTo(fp2) > 0);
            Assert.True(fp2.CompareTo(fp1) < 0);

            fp2.Touch(31);
            Assert.Equal(0, fp1.CompareTo(fp2));
            Assert.Equal(0, fp2.CompareTo(fp1));

            fp2.Touch(64);
            Assert.True(fp1.CompareTo(fp2) < 0);
            Assert.True(fp2.CompareTo(fp1) > 0);
        }

        [Fact]
        public void TestEquality()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            // Reference first
            Assert.True(fp1.Equals(fp1));
            Assert.True(fp2.Equals(fp2));
            Assert.True(fp3.Equals(fp3));
            Assert.True(fp4.Equals(fp4));

            // Type second
            Assert.False(fp1.Equals(new Object()));

            // Length third

            Assert.False(fp3.Equals(fp1));
            Assert.False(fp1.Equals(fp3));

            Assert.False(fp4.Equals(fp2));
            Assert.False(fp2.Equals(fp4));

            // Bits forth

            Assert.True(fp1.Equals(fp2));
            Assert.True(fp2.Equals(fp1));

            fp1.Touch(32);
            Assert.False(fp1.Equals(fp2));
            Assert.False(fp2.Equals(fp1));

            fp2.Touch(32);
            Assert.True(fp1.Equals(fp2));
            Assert.True(fp2.Equals(fp1));

            // Length <= 32
            var fp5 = new Fingerprint(7);
            var fp6 = new Fingerprint(7);
            fp5.Touch(0);
            Assert.False(fp5.Equals(fp6));
            fp6.Touch(0);
            Assert.True(fp5.Equals(fp6));
        }

        [Fact]
        public void TestHashing()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            Assert.Equal(fp1.GetHashCode(), fp2.GetHashCode());
            Assert.NotEqual(fp1.GetHashCode(), fp3.GetHashCode());
            Assert.NotEqual(fp2.GetHashCode(), fp4.GetHashCode());

            fp1.Touch(32);
            Assert.NotEqual(fp1.GetHashCode(), fp2.GetHashCode());
            fp2.Touch(32);
            Assert.Equal(fp1.GetHashCode(), fp2.GetHashCode());
        }

        [Fact]
        public void TestEquivalence()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);
            Fingerprint fp3 = new Fingerprint(64);
            Fingerprint fp4 = new Fingerprint(66);

            // Reference first
            Assert.True(fp1.Equivalent(fp1));
            Assert.True(fp2.Equivalent(fp2));
            Assert.True(fp3.Equivalent(fp3));
            Assert.True(fp4.Equivalent(fp4));

            // Length second

            Assert.True(fp3.Equivalent(fp1));
            Assert.False(fp1.Equivalent(fp3));

            Assert.False(fp4.Equivalent(fp2));
            Assert.True(fp2.Equivalent(fp4));

            // Bits third

            Assert.True(fp1.Equivalent(fp2));
            Assert.True(fp2.Equivalent(fp1));

            fp1.Touch(32);
            Assert.False(fp1.Equivalent(fp2));
            Assert.True(fp2.Equivalent(fp1));

            fp2.Touch(32);
            Assert.True(fp1.Equivalent(fp2));
            Assert.True(fp2.Equivalent(fp1));

            fp2.Touch(31);
            Assert.True(fp1.Equivalent(fp2));
            Assert.False(fp2.Equivalent(fp1));

            fp4.Touch(31);
            fp4.Touch(32);
            fp4.Touch(33);
            Assert.True(fp2.Equivalent(fp4));
            Assert.False(fp4.Equivalent(fp2));
        }

        [Fact]
        public void TestSerialization()
        {
            Fingerprint fp1 = new Fingerprint(65);
            Fingerprint fp2 = new Fingerprint(65);

            fp1.Touch(0);
            fp1.Touch(2);
            fp1.Touch(31);
            fp1.Touch(32);

            Assert.False(fp2.Equals(fp1));

            var buffer = new x2net.Buffer();
            fp1.Serialize(new Serializer(buffer));
            buffer.Rewind();
            fp2.Deserialize(new Deserializer(buffer));

            Assert.True(fp2.Equals(fp1));
        }
    }
}
