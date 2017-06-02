using System;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class PoolTest
    {
        class Foo
        {
            public int bar = 0;
        }

        [Fact]
        public void TestCreation()
        {
            var p0 = new Pool<Foo>();
            Assert.Equal(0, p0.Capacity);
            Assert.Equal(0, p0.Count);

            var p1 = new Pool<Foo>(1);
            Assert.Equal(1, p1.Capacity);
            Assert.Equal(0, p1.Count);
        }

        [Fact]
        public void TestNullPush()
        {
            var p = new Pool<Foo>();
            Assert.Throws<ArgumentNullException>(() => { p.Push(null); });
        }

        [Fact]
        public void TestPushPop()
        {
            var p0 = new Pool<Foo>();
            var p1 = new Pool<Foo>(1);

            var f = new Foo();
            var g = new Foo();

            p0.Push(f);
            Assert.Equal(1, p0.Count);
            p0.Push(g);
            Assert.Equal(2, p0.Count);

            p1.Push(f);
            Assert.Equal(1, p1.Count);
            // capacity overflow
            p1.Push(g);
            Assert.Equal(1, p1.Count);

            Foo g1 = p0.Pop();
            Assert.Same(g, g1);
            Foo f1 = p0.Pop();
            Assert.Same(f, f1);

            Foo f2 = p1.Pop();
            Assert.Same(f, f2);
            // pop underflow
            Foo g2 = p1.Pop();
            Assert.Same(null, g2);
        }
    }
}
