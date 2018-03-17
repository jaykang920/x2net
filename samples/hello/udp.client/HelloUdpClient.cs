using System;
using System.Net;

using x2net;

namespace hello
{
    public class HelloUdpClient : UdpLink
    {
        private static int peerHandle;

        public HelloUdpClient() : base("HelloClient")
        {
            // Uncomment the following block to enable channel encryption.
            //BufferTransform = new BlockCipher();
        }

        protected override void Setup()
        {
            EventFactory.Register<HelloResp>();
            Bind(new HelloReq(), (e) => {
                e._Handle = peerHandle;
                Send(e);
            });

            Bind(7891);
            Listen();

            peerHandle = AddEndPoint(new IPEndPoint(IPAddress.Loopback, 7890));
        }

        public static void Main()
        {
            Config.TraceLevel = TraceLevel.Debug;
            Trace.Handler = (level, message) => { Console.WriteLine(message); };

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new OutputCase())
                    .Add(new HelloUdpClient()));

            using (new Hub.Flows().Startup())
            {
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == "bye")
                    {
                        break;
                    }
                    new HelloReq() { Name = input }.Post();
                }
            }
        }
    }
}
