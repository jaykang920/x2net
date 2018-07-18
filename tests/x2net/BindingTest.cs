using System;
using System.Collections.Generic;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class BindingTest
    {
        [Fact]
        public void TestBinding()
        {
            Binding binding = new Binding();
            var equivalent = new EventEquivalent();

            binding.Bind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));

            var e1 = new SampleEvent1();
            var e2 = new SampleEvent1 { Foo = 1 };
            var e3 = new SampleEvent1 { Foo = 1, Bar = "bar" };

            List<Handler> handlerChain = new List<Handler>();

            handlerChain.Clear();
            Assert.Equal(1, binding.BuildHandlerChain(e1, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            handlerChain.Clear();
            Assert.Equal(1, binding.BuildHandlerChain(e2, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            handlerChain.Clear();
            Assert.Equal(1, binding.BuildHandlerChain(e3, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);

            binding.Unbind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));

            handlerChain.Clear();
            Assert.Equal(0, binding.BuildHandlerChain(e1, equivalent, handlerChain));
            handlerChain.Clear();
            Assert.Equal(0, binding.BuildHandlerChain(e2, equivalent, handlerChain));
            handlerChain.Clear();
            Assert.Equal(0, binding.BuildHandlerChain(e3, equivalent, handlerChain));
        }

        [Fact]
        public void TestDuplicateBinding()
        {
            Binding binding = new Binding();
            var equivalent = new EventEquivalent();
            List<Handler> handlerChain = new List<Handler>();

            binding.Bind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSampleEvent1));
            binding.Bind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSampleEvent1));

            binding.Unbind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSampleEvent1));

            Assert.Equal(0, binding.BuildHandlerChain(new SampleEvent1 { Foo = 1 }, equivalent, handlerChain));

            // with EventSink

            var sink = new SampleEventSink();
            sink.Bind(new SampleEvent1 { Foo = 1 }, sink.OnSampleEvent1);
            sink.Bind(new SampleEvent1 { Foo = 1 }, sink.OnSampleEvent1);

            sink.Unbind(new SampleEvent1 { Foo = 1 }, sink.OnSampleEvent1);

            handlerChain.Clear();
            Assert.Equal(0, binding.BuildHandlerChain(new SampleEvent1 { Foo = 1 }, equivalent, handlerChain));
        }

        [Fact]
        public void TestDuplicateUnbinding()
        {
            Binding binding = new Binding();
            var equivalent = new EventEquivalent();
            List<Handler> handlerChain = new List<Handler>();

            binding.Bind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSampleEvent1));
            binding.Bind(new SampleEvent1 { Foo = 2 }, new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1));

            binding.Unbind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSampleEvent1));
            binding.Unbind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSampleEvent1));

            handlerChain.Clear();
            Assert.Equal(0, binding.BuildHandlerChain(new SampleEvent1 { Foo = 1 }, equivalent, handlerChain));
            Assert.Equal(1, binding.BuildHandlerChain(new SampleEvent1 { Foo = 2 }, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[0]);

            // with EventSink

            var sink = new SampleEventSink();
            sink.Bind(new SampleEvent1 { Foo = 1 }, sink.OnSampleEvent1);

            sink.Unbind(new SampleEvent1 { Foo = 1 }, sink.OnSampleEvent1);
            sink.Bind(new SampleEvent1 { Foo = 1 }, sink.OnSampleEvent1);

            handlerChain.Clear();
            Assert.Equal(0, binding.BuildHandlerChain(new SampleEvent1 { Foo = 1 }, equivalent, handlerChain));
            Assert.Equal(1, binding.BuildHandlerChain(new SampleEvent1 { Foo = 2 }, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[0]);
        }

        [Fact]
        public void TestHandlerChainBuilding()
        {
            Binding binding = new Binding();
            var equivalent = new EventEquivalent();

            binding.Bind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));
            binding.Bind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1));
            binding.Bind(new Event(), new MethodHandler<Event>(OnEvent));

            var e1 = new SampleEvent1();
            var e2 = new SampleEvent1 { Foo = 1 };
            var e3 = new SampleEvent1 { Foo = 1, Bar = "bar" };
            var e4 = new SampleEvent1 { Foo = 2 };
            var e5 = new SampleEvent2 { Foo = 1, Bar = "bar" };
            var e6 = new SampleEvent2 { Foo = 2, Bar = "bar" };

            List<Handler> handlerChain = new List<Handler>();

            handlerChain.Clear();
            Assert.Equal(2, binding.BuildHandlerChain(e1, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.Equal(new MethodHandler<Event>(OnEvent), handlerChain[1]);

            handlerChain.Clear();
            Assert.Equal(3, binding.BuildHandlerChain(e2, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[1]);
            Assert.Equal(new MethodHandler<Event>(OnEvent), handlerChain[2]);

            handlerChain.Clear();
            Assert.Equal(3, binding.BuildHandlerChain(e3, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[1]);
            Assert.Equal(new MethodHandler<Event>(OnEvent), handlerChain[2]);

            handlerChain.Clear();
            Assert.Equal(2, binding.BuildHandlerChain(e4, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.Equal(new MethodHandler<Event>(OnEvent), handlerChain[1]);

            handlerChain.Clear();
            Assert.Equal(3, binding.BuildHandlerChain(e5, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[1]);
            Assert.Equal(new MethodHandler<Event>(OnEvent), handlerChain[2]);

            handlerChain.Clear();
            Assert.Equal(2, binding.BuildHandlerChain(e6, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.Equal(new MethodHandler<Event>(OnEvent), handlerChain[1]);

            binding.Unbind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));

            handlerChain.Clear();
            Assert.Equal(1, binding.BuildHandlerChain(e1, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<Event>(OnEvent), handlerChain[0]);

            binding.Unbind(new Event(), new MethodHandler<Event>(OnEvent));

            handlerChain.Clear();
            Assert.Equal(1, binding.BuildHandlerChain(e2, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[0]);

            handlerChain.Clear();
            Assert.Equal(1, binding.BuildHandlerChain(e3, equivalent, handlerChain));
            Assert.Equal(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[0]);

            handlerChain.Clear();
            Assert.Equal(0, binding.BuildHandlerChain(e4, equivalent, handlerChain));
        }

        [Fact]
        public void TestBasicPerformance()
        {
            Binding binding = new Binding();
            var equivalent = new EventEquivalent();
            List<Handler> handlerChain = new List<Handler>();

            binding.Bind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSampleEvent1));

            // const int testCount = 1000000;
            const int testCount = 1;

            for (var i = 0; i < testCount; ++i)
            {
                binding.BuildHandlerChain(new SampleEvent1 {Foo = 1}, equivalent, handlerChain);
            }
            // 1,000,000 counts in 342 ms in release mode
        }

        void OnEvent(Event e)
        {
        }

        void OnSampleEvent1(SampleEvent1 e)
        {
        }

        void OnSpecificSampleEvent1(SampleEvent1 e)
        {
        }

        class SampleEventSink : EventSink
        {
            public void OnSampleEvent1(SampleEvent1 e)
            {
            }

            public void OnSpecificSampleEvent1(SampleEvent1 e)
            {
            }
        }
    }
}
