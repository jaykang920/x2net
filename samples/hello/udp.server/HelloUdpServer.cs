using System;
using System.Net;

using x2net;

namespace hello
{
    public class HelloUdpServer : AsyncUdpLink
    {
        private static int peerHandle;

        public HelloUdpServer() : base("HelloServer")
        {
            // Uncomment the following block to enable channel encryption.
            //BufferTransform = new BlockCipher();
        }

        protected override void Setup()
        {
            EventFactory.Register<HelloReq>();
            Bind(new HelloResp(), (e) => {
                e._Handle = peerHandle;
                Send(e);
            });

            Bind(7890);
            Listen();

            peerHandle = AddEndPoint(new IPEndPoint(IPAddress.Loopback, 7891));
        }

        public static void Main()
        {
            Config.TraceLevel = TraceLevel.Debug;
            Trace.Handler = (level, message) => { Console.WriteLine(message); };

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new HelloCase())
                    .Add(new HelloUdpServer()));

            using (new Hub.Flows().Startup())
            {
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == "bye")
                    {
                        break;
                    }
                }
            }
        }
    }
}
