using System;

using x2net;

namespace hello
{
    public class OutputCase : Case
    {
        protected override void Setup()
        {
            Bind(new HelloResp(), OnHelloResp);
        }

        void OnHelloResp(HelloResp e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
