using System;

using x2net;

namespace hello
{
    public class HelloTcpServer : AsyncTcpServer
    {
        public HelloTcpServer() : base("HelloServer")
        {
            // Uncomment the following block to enable channel encryption.
            /*
            ChannelStrategy = new BufferTransformStrategy {
                BufferTransform = new BlockCipher()
            };
            */
            // Uncomment the following line to enable heartbeat-based keepalive.
            //HeartbeatStrategy = new ServerKeepaliveStrategy();
        }

        protected override void Setup()
        {
            EventFactory.Register<HelloReq>();
            Bind(new HelloResp(), Send);
            Listen(6789);
        }

        public static void Main()
        {
            Config.TraceLevel = TraceLevel.Debug;
            Trace.Handler = (level, message) => { Console.WriteLine(message); };

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new HelloCase())
                    .Add(new HelloTcpServer()));

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
