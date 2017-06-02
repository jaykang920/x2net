using System;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class HandlerTest
    {
        class IntBox
        {
            public static int StaticValue { get; set; }
            public int Value { get; set; }

            public static void StaticIncrement(Event e)
            {
                ++StaticValue;
            }

            public static void StaticDecrement(Event e)
            {
                --StaticValue;
            }

            public void Increment(Event e)
            {
                ++Value;
            }

            public void Decrement(Event e)
            {
                --Value;
            }
        }

        private IntBox intBox1 = new IntBox();
        private IntBox intBox2 = new IntBox();

        public HandlerTest()
        {
            IntBox.StaticValue = 0;
            intBox1.Value = 0;
            intBox2.Value = 0;
        }

        [Fact]
        public void TestStaticMethods()
        {
            var handler1 = new MethodHandler<Event>(IntBox.StaticIncrement);
            var handler2 = new MethodHandler<Event>(IntBox.StaticIncrement);
            var handler3 = new MethodHandler<Event>(IntBox.StaticDecrement);

            // Properties
            Assert.True(handler1.Action.Equals(handler2.Action));
            Assert.False(handler2.Action.Equals(handler3.Action));

            // Invocation
            Event e = new Event();
            handler1.Invoke(e);
            Assert.Equal(1, IntBox.StaticValue);
            handler2.Invoke(e);
            Assert.Equal(2, IntBox.StaticValue);
            handler3.Invoke(e);
            Assert.Equal(1, IntBox.StaticValue);

            // Equality
            Assert.True(handler1.Equals(handler2));
            Assert.False(handler2.Equals(handler3));
        }

        [Fact]
        public void TestInstanceMethods()
        {
            var handler1 = new MethodHandler<Event>(intBox1.Increment);
            var handler2 = new MethodHandler<Event>(intBox1.Increment);
            var handler3 = new MethodHandler<Event>(intBox1.Decrement);
            var handler4 = new MethodHandler<Event>(intBox2.Decrement);

            // Properties
            Assert.True(handler1.Action.Equals(handler2.Action));
            Assert.False(handler2.Action.Equals(handler3.Action));
            Assert.False(handler3.Action.Equals(handler4.Action));

            // Invocation
            Event e = new Event();
            handler1.Invoke(e);
            Assert.Equal(1, intBox1.Value);
            handler2.Invoke(e);
            Assert.Equal(2, intBox1.Value);
            handler3.Invoke(e);
            Assert.Equal(1, intBox1.Value);
            Assert.Equal(0, intBox2.Value);
            handler4.Invoke(e);
            Assert.Equal(1, intBox1.Value);
            Assert.Equal(-1, intBox2.Value);

            // Equality
            Assert.True(handler1.Equals(handler2));
            Assert.False(handler2.Equals(handler3));
            Assert.False(handler3.Equals(handler4));
        }
    }
}
