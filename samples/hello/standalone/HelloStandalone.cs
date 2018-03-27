using System;

using x2net;

namespace hello
{
    public class HelloStandalone
    {
        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new HelloCase())
                    .Add(new OutputCase()));

            using (new Hub.Flows().Startup())
            {
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == "bye")
                    {
                        break;
                    }
                    new HelloReq { Name = input }.Post();
                }
            }
        }
    }
}
