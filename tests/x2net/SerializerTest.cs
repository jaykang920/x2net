using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

using x2net;

namespace x2net.tests
{
    public class SerializerTest
    {
        [Fact]
        public void TestFloat32()
        {
            //using (var stream = new MemoryStream())
            {
                float f;
                var stream = new x2net.Buffer();
                Serializer serializer = new Serializer(stream);
                Deserializer deserializer = new Deserializer(stream);

                // Boundary value tests

                serializer.Write(0.0F);
                serializer.Write(Single.Epsilon);
                serializer.Write(Single.MinValue);
                serializer.Write(Single.MaxValue);
                serializer.Write(Single.NegativeInfinity);
                serializer.Write(Single.PositiveInfinity);
                serializer.Write(Single.NaN);

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                deserializer.Read(out f);
                Assert.Equal(0.0F, f);
                deserializer.Read(out f);
                Assert.Equal(Single.Epsilon, f);
                deserializer.Read(out f);
                Assert.Equal(Single.MinValue, f);
                deserializer.Read(out f);
                Assert.Equal(Single.MaxValue, f);
                deserializer.Read(out f);
                Assert.Equal(Single.NegativeInfinity, f);
                deserializer.Read(out f);
                Assert.Equal(Single.PositiveInfinity, f);
                deserializer.Read(out f);
                Assert.Equal(Single.NaN, f);

                stream.Trim();
                //stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0.001234F);
                serializer.Write(8765.4321F);

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                deserializer.Read(out f);
                Assert.Equal(0.001234F, f);
                deserializer.Read(out f);
                Assert.Equal(8765.4321F, f);
            }
        }

        [Fact]
        public void TestFloat64()
        {
            //using (var stream = new MemoryStream())
            {
                double d;
                var stream = new x2net.Buffer();
                Serializer serializer = new Serializer(stream);
                Deserializer deserializer = new Deserializer(stream);

                // Boundary value tests

                serializer.Write(0.0);
                serializer.Write(Double.Epsilon);
                serializer.Write(Double.MinValue);
                serializer.Write(Double.MaxValue);
                serializer.Write(Double.NegativeInfinity);
                serializer.Write(Double.PositiveInfinity);
                serializer.Write(Double.NaN);

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                deserializer.Read(out d);
                Assert.Equal(0.0, d);
                deserializer.Read(out d);
                Assert.Equal(Double.Epsilon, d);
                deserializer.Read(out d);
                Assert.Equal(Double.MinValue, d);
                deserializer.Read(out d);
                Assert.Equal(Double.MaxValue, d);
                deserializer.Read(out d);
                Assert.Equal(Double.NegativeInfinity, d);
                deserializer.Read(out d);
                Assert.Equal(Double.PositiveInfinity, d);
                deserializer.Read(out d);
                Assert.Equal(Double.NaN, d);

                stream.Trim();
                //stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0.001234);
                serializer.Write(8765.4321);

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                deserializer.Read(out d);
                Assert.Equal(0.001234, d);
                deserializer.Read(out d);
                Assert.Equal(8765.4321, d);
            }
        }

        [Fact]
        public void TestVariableLengthInt32()
        {
            //using (var stream = new MemoryStream())
            {
                int i, bytes;
                var stream = new x2net.Buffer();
                Serializer serializer = new Serializer(stream);
                Deserializer deserializer = new Deserializer(stream);

                // Boundary value tests

                serializer.Write(0);
                serializer.Write(-1);
                serializer.Write(Int32.MaxValue);
                serializer.Write(Int32.MinValue);

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                bytes = deserializer.Read(out i);
                Assert.Equal(1, bytes);
                Assert.Equal(0, i);

                bytes = deserializer.Read(out i);
                Assert.Equal(1, bytes);
                Assert.Equal(-1, i);

                bytes = deserializer.Read(out i);
                Assert.Equal(5, bytes);
                Assert.Equal(Int32.MaxValue, i);

                bytes = deserializer.Read(out i);
                Assert.Equal(5, bytes);
                Assert.Equal(Int32.MinValue, i);

                stream.Trim();
                //stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0x00003f80 >> 1);  // 2
                serializer.Write(0x001fc000 >> 1);  // 3
                serializer.Write(0x0fe00000 >> 1);  // 4

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                bytes = deserializer.Read(out i);
                Assert.Equal(2, bytes);
                Assert.Equal(0x00003f80 >> 1, i);

                bytes = deserializer.Read(out i);
                Assert.Equal(3, bytes);
                Assert.Equal(0x001fc000 >> 1, i);

                bytes = deserializer.Read(out i);
                Assert.Equal(4, bytes);
                Assert.Equal(0x0fe00000 >> 1, i);
            }
        }

        [Fact]
        public void TestVariableLengthInt64()
        {
            //using (var stream = new MemoryStream())
            {
                var stream = new x2net.Buffer();
                Serializer serializer = new Serializer(stream);
                Deserializer deserializer = new Deserializer(stream);

                // Boundary value tests

                serializer.Write(0L);
                serializer.Write(-1L);
                serializer.Write(Int64.MaxValue);
                serializer.Write(Int64.MinValue);

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                long l;
                long bytes = deserializer.Read(out l);
                Assert.Equal(1, bytes);
                Assert.Equal(0L, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(1, bytes);
                Assert.Equal(-1L, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(10, bytes);
                Assert.Equal(Int64.MaxValue, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(10, bytes);
                Assert.Equal(Int64.MinValue, l);

                stream.Trim();
                //stream.SetLength(0);

                // Intermediate value tests

                serializer.Write(0x00003f80L >> 1);  // 2
                serializer.Write(0x001fc000L >> 1);  // 3
                serializer.Write(0x0fe00000L >> 1);  // 4
                serializer.Write(0x00000007f0000000L >> 1);  // 5
                serializer.Write(0x000003f800000000L >> 1);  // 6
                serializer.Write(0x0001fc0000000000L >> 1);  // 7
                serializer.Write(0x00fe000000000000L >> 1);  // 8
                serializer.Write(0x7f00000000000000L >> 1);  // 9

                stream.Rewind();
                //stream.Seek(0, SeekOrigin.Begin);

                bytes = deserializer.Read(out l);
                Assert.Equal(2, bytes);
                Assert.Equal(0x00003f80L >> 1, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(3, bytes);
                Assert.Equal(0x001fc000L >> 1, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(4, bytes);
                Assert.Equal(0x0fe00000L >> 1, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(5, bytes);
                Assert.Equal(0x00000007f0000000L >> 1, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(6, bytes);
                Assert.Equal(0x000003f800000000L >> 1, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(7, bytes);
                Assert.Equal(0x0001fc0000000000L >> 1, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(8, bytes);
                Assert.Equal(0x00fe000000000000L >> 1, l);

                bytes = deserializer.Read(out l);
                Assert.Equal(9, bytes);
                Assert.Equal(0x7f00000000000000L >> 1, l);
            }
        }

        [Fact]
        public void TestCollections()
        {
            var buffer = new x2net.Buffer();

            // List<int>
            var list = new List<int> { 1, 2, 3 };

            var serializer = new Serializer(buffer);
            serializer.Write(list);

            buffer.Rewind();
            var deserializer = new Deserializer(buffer);

            List<int> deserialized;
            deserializer.Read(out deserialized);

            Assert.NotNull(deserialized);
            Assert.True(list.EqualsExtended(deserialized));

            // List<string>
            var list2 = new List<string> { "one", "two", "three" };

            buffer.Reset();
            serializer = new Serializer(buffer);
            serializer.Write(list2);

            buffer.Rewind();
            deserializer = new Deserializer(buffer);

            List<string> deserialized2;
            deserializer.Read(out deserialized2);

            Assert.NotNull(deserialized2);
            Assert.True(list2.EqualsExtended(deserialized2));

            // List<List<int>>
            var list3 = new List<List<int>>();
            list3.Add(list);
            list3.Add(list);
            list3.Add(list);

            buffer.Reset();
            serializer = new Serializer(buffer);
            serializer.Write(list3);

            buffer.Rewind();
            deserializer = new Deserializer(buffer);

            List<List<int>> deserialized3;
            deserializer.Read(out deserialized3);

            Assert.NotNull(deserialized2);
            Assert.Equal(list3[0][0], deserialized3[0][0]);
            Assert.Equal(list3[1][1], deserialized3[1][1]);
            Assert.Equal(list3[2][1], deserialized3[2][1]);
            Assert.Equal(list3[2][2], deserialized3[2][2]);

#if NET40
            // Map
            var map = new Dictionary<int, string>();
            map[1] = "one";
            map[2] = "two";
            map[3] = "three";

            buffer.Reset();
            serializer = new Serializer(buffer);
            serializer.Write(map);

            buffer.Rewind();
            deserializer = new Deserializer(buffer);

            Dictionary<int, string> deserialized4;
            deserializer.Read(out deserialized4);

            Assert.NotNull(deserialized4);
            Assert.Equal(map[1], deserialized4[1]);

            // Map of lists
            var map2 = new Dictionary<string, List<SampleCell1>>();
            map2.Add("key1",
                new List<SampleCell1> { new SampleCell1 { Foo = 1 } });

            buffer.Reset();
            serializer = new Serializer(buffer);
            serializer.Write(map2);

            buffer.Rewind();
            deserializer = new Deserializer(buffer);

            Dictionary<string, List<SampleCell1>> deserialized5;
            deserializer.Read(out deserialized5);

            Assert.NotNull(deserialized5);
            var key = "key1";
            Assert.Equal(map2[key][0].Foo, deserialized5[key][0].Foo);
#endif
        }

        [Fact]
        public void TestPartialSerialization()
        {
            var eventFactory = new EventFactory();
            eventFactory.Register<SampleEvent5>();

            var buffer = new x2net.Buffer();

            var cell1 = new SampleCell1 {  // base
                Foo = 9,
                Bar = "hello"
            };
            var cell2 = new SampleCell2 {  // derived
                Foo = 9,
                Bar = "hello",
                Baz = true
            };

            var event1 = new SampleEvent5 {

                // base > base > base
                SampleCell = cell1
            };  // has base
            Serializer serializer = new Serializer(buffer);
            serializer.Write(event1.GetTypeId());
            event1.Serialize(serializer);

            long bufferLength = buffer.Length;

            buffer.Rewind();
            Deserializer deserializer = new Deserializer(buffer);
            int typeId;
            deserializer.Read(out typeId);
            var retrieved = eventFactory.Create(typeId);
            retrieved.Deserialize(deserializer);

            var event11 = retrieved as SampleEvent5;
            Assert.NotNull(event11);

            Assert.Equal(event1.SampleCell.Foo, event11.SampleCell.Foo);
            Assert.Equal(event1.SampleCell.Bar, event11.SampleCell.Bar);

            buffer.Reset();

            // derived > base > base
            event1.SampleCell = cell2;  // base <= derived
            serializer = new Serializer(buffer);
            serializer.Write(event1.GetTypeId());
            event1.Serialize(serializer);

            Assert.Equal(bufferLength, buffer.Length);

            {
                var event2 = new SampleEvent6 {
                    SampleCell = cell2  // derived <= derived
                };  // has derived
                var buffer2 = new x2net.Buffer();
                serializer = new Serializer(buffer2);
                serializer.Write(event2.GetTypeId());
                event2.Serialize(serializer);
                Assert.True(buffer2.Length > buffer.Length);
            }

            buffer.Rewind();
            deserializer = new Deserializer(buffer);
            deserializer.Read(out typeId);
            retrieved = eventFactory.Create(typeId);
            retrieved.Deserialize(deserializer);

            var event12 = retrieved as SampleEvent5;
            Assert.NotNull(event12);

            Assert.True(event12.SampleCell is SampleCell1);

            Assert.Equal(event1.SampleCell.Foo, event12.SampleCell.Foo);
            Assert.Equal(event1.SampleCell.Bar, event12.SampleCell.Bar);
        }

        [Fact]
        public void TestFullSerialization()
        {
            var eventFactory = new EventFactory();
            eventFactory.Register<SampleEvent1>();
            eventFactory.Register<SampleEvent2>();
            eventFactory.Register<SampleEvent7>();

            var buffer = new x2net.Buffer();

            var event1 = new SampleEvent1 {  // base
                Foo = 9,
                Bar = "hello"
            };
            var event2 = new SampleEvent2 {  // derived
                Foo = 9,
                Bar = "hello",
                Baz = true
            };

            var e = new SampleEvent7 {
                // base > base > base
                SampleEvent = event1
            };  // has base
            Serializer serializer = new Serializer(buffer);
            serializer.Write(e.GetTypeId());
            e.Serialize(serializer);

            long bufferLength = buffer.Length;

            buffer.Rewind();
            Deserializer deserializer = new Deserializer(buffer, eventFactory);
            int typeId;
            deserializer.Read(out typeId);
            var retrieved = eventFactory.Create(typeId);
            retrieved.Deserialize(deserializer);

            var e2 = retrieved as SampleEvent7;
            Assert.NotNull(e2);

            Assert.Equal(e.SampleEvent.Foo, e2.SampleEvent.Foo);
            Assert.Equal(e.SampleEvent.Bar, e2.SampleEvent.Bar);

            buffer.Reset();

            // derived > base > base
            e.SampleEvent = event2;  // base <= derived
            serializer = new Serializer(buffer);
            serializer.Write(e.GetTypeId());
            e.Serialize(serializer);

            Assert.True(bufferLength < buffer.Length);

            {
                var e3 = new SampleEvent8 {
                    SampleEvent = event2  // derived <= derived
                };  // has derived
                var buffer2 = new x2net.Buffer();
                serializer = new Serializer(buffer2);
                serializer.Write(e3.GetTypeId());
                e3.Serialize(serializer);
                Assert.Equal(buffer2.Length, buffer.Length);
            }

            buffer.Rewind();
            deserializer = new Deserializer(buffer, eventFactory);
            deserializer.Read(out typeId);
            retrieved = eventFactory.Create(typeId);
            retrieved.Deserialize(deserializer);

            var e4 = retrieved as SampleEvent7;
            Assert.NotNull(e4);

            var deserialized = e4.SampleEvent as SampleEvent2;
            Assert.NotNull(deserialized);
            Assert.True(deserialized.Baz);

            Assert.Equal(e.SampleEvent.Foo, e4.SampleEvent.Foo);
            Assert.Equal(e.SampleEvent.Bar, e4.SampleEvent.Bar);
        }
    }
}
