using System;

using x2net;

namespace hello
{
    public class HelloCase : Case
    {
        protected override void Setup()
        {
            Bind(new HelloReq(), OnHelloReq);
        }

        void OnHelloReq(HelloReq req)
        {
            new HelloResp() {
                Message = String.Format("Hello, {0}!", req.Name)
            }.InResponseOf(req).Post();
        }
    }
}
