using System;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class CellTest
    {
        [Fact]
        public void TestCreation()
        {
            Cell cell1 = new SampleCell1();
            Cell cell2 = new SampleCell2();
            Cell cell3 = new SampleCell3();
            Cell cell4 = new SampleCell4();

            // Static construction

            var tag = cell1.GetTypeTag();
            Assert.Null(tag.Base);
            Assert.Equal(cell1.GetType(), tag.RuntimeType);
            Assert.Equal(2, tag.NumProps);
            Assert.Equal(0, tag.Offset);

            tag = cell2.GetTypeTag();
            Assert.Equal(cell1.GetTypeTag(), tag.Base);
            Assert.Equal(cell2.GetType(), tag.RuntimeType);
            Assert.Equal(1, tag.NumProps);
            Assert.Equal(2, tag.Offset);

            tag = cell3.GetTypeTag();
            Assert.Equal(cell1.GetTypeTag(), tag.Base);
            Assert.Equal(cell3.GetType(), tag.RuntimeType);
            Assert.Equal(1, tag.NumProps);
            Assert.Equal(2, tag.Offset);

            tag = cell4.GetTypeTag();
            Assert.Equal(cell2.GetTypeTag(), tag.Base);
            Assert.Equal(cell4.GetType(), tag.RuntimeType);
            Assert.Equal(1, tag.NumProps);
            Assert.Equal(3, tag.Offset);

            // Fingerprint length
            Assert.Equal(2, cell1.GetFingerprint().Length);
            Assert.Equal(3, cell2.GetFingerprint().Length);
            Assert.Equal(3, cell3.GetFingerprint().Length);
            Assert.Equal(4, cell4.GetFingerprint().Length);
        }

        [Fact]
        public void TestEquality()
        {
            var cell1 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell2 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell3 = new SampleCell1 { Foo = 1, Bar = "foo" };
            var cell4 = new SampleCell2 { Foo = 1, Bar = "bar" };
            var cell5 = new SampleCell3 { Foo = 1, Bar = "bar" };
            var cell6 = new SampleCell4 { Foo = 1, Bar = "bar" };
            var cell7 = new SampleCell4 { Foo = 1, Bar = "bar" };

            Assert.True(cell1.Equals(cell2));
            Assert.True(cell2.Equals(cell1));
            Assert.False(cell1.Equals(cell3));
            Assert.False(cell3.Equals(cell1));

            Assert.False(cell1.Equals(cell4));
            Assert.False(cell1.Equals(cell5));
            Assert.False(cell1.Equals(cell6));
            Assert.False(cell1.Equals(cell7));

            Assert.True(cell6.Equals(cell7));
        }

        [Fact]
        public void TestHashing()
        {
            var cell1 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell2 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell3 = new SampleCell1 { Foo = 1, Bar = "foo" };
            var cell4 = new SampleCell2 { Foo = 1, Bar = "bar" };
            var cell5 = new SampleCell3 { Foo = 1, Bar = "bar" };
            var cell6 = new SampleCell4 { Foo = 1, Bar = "bar" };
            var cell7 = new SampleCell4 { Foo = 1, Bar = "bar" };

            Assert.Equal(cell1.GetHashCode(), cell2.GetHashCode());
            Assert.NotEqual(cell1.GetHashCode(), cell3.GetHashCode());

            Assert.Equal(cell1.GetHashCode(), cell4.GetHashCode());
            Assert.Equal(cell1.GetHashCode(), cell5.GetHashCode());
            Assert.Equal(cell1.GetHashCode(), cell6.GetHashCode());
            Assert.Equal(cell1.GetHashCode(), cell7.GetHashCode());

            Assert.Equal(cell6.GetHashCode(), cell7.GetHashCode());

            cell6.Quux = true;
            Assert.NotEqual(cell6.GetHashCode(), cell7.GetHashCode());
            cell7.Quux = true;
            Assert.Equal(cell6.GetHashCode(), cell7.GetHashCode());
        }

        [Fact]
        public void TestEquivalence()
        {
            var cell1 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell2 = new SampleCell1 { Foo = 1, Bar = "bar" };
            var cell3 = new SampleCell1 { Foo = 1, Bar = "foo" };
            var cell4 = new SampleCell2 { Foo = 1, Bar = "bar" };
            var cell5 = new SampleCell3 { Foo = 1, Bar = "bar" };
            var cell6 = new SampleCell4 { Foo = 1, Bar = "bar" };
            var cell7 = new SampleCell4 { Foo = 1, Bar = "bar" };

            // Reference first
            Assert.True(cell1.Equivalent(cell1));
            Assert.True(cell2.Equivalent(cell2));
            Assert.True(cell3.Equivalent(cell3));
            Assert.True(cell4.Equivalent(cell4));

            // Fingerprint second

            Assert.True(cell1.Equivalent(cell4));
            Assert.False(cell4.Equivalent(cell1));

            Assert.True(cell6.Equivalent(cell7));
            Assert.True(cell7.Equivalent(cell6));
            cell7.Quux = true;
            Assert.True(cell6.Equivalent(cell7));
            Assert.False(cell7.Equivalent(cell6));

            // Contents third

            cell4.Bar = "foo";
            Assert.False(cell1.Equivalent(cell4));
            Assert.False(cell4.Equivalent(cell1));

            cell6.Quux = false;
            Assert.False(cell6.Equivalent(cell7));
            Assert.False(cell7.Equivalent(cell6));
            cell6.Quux = true;
            Assert.True(cell6.Equivalent(cell7));
            Assert.True(cell7.Equivalent(cell6));
        }
    }
}
