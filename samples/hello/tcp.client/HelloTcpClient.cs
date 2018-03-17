using System;

using x2net;

namespace hello
{
    public class HelloTcpClient : TcpClient
    {
        public HelloTcpClient() : base("HelloClient")
        {
            // Uncomment the following block to enable channel encryption.
            /*
            ChannelStrategy = new BufferTransformStrategy {
                BufferTransform = new BlockCipher()
            };
            */
            // Uncomment the following line to enable heartbeat-based keepalive.
            //HeartbeatStrategy = new ClientKeepaliveStrategy();
        }

        protected override void Setup()
        {
            EventFactory.Register<HelloResp>();
            Bind(new HelloReq(), Send);
            Connect("127.0.0.1", 6789);
        }

        public static void Main()
        {
            Config.TraceLevel = TraceLevel.Debug;
            Trace.Handler = (level, message) => { Console.WriteLine(message); };

            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new OutputCase())
                    .Add(new HelloTcpClient()));

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
