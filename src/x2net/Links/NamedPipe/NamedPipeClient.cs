// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

#if NET45

using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace x2net
{
    public class NamedPipeClient : ClientLink
    {
        private const string localhost = ".";

        CancellationTokenSource cts;

        public NamedPipeClient(string name)
            : base(name)
        {
            cts = new CancellationTokenSource();
        }

        public void Connect()
        {
            Task.Run(() => {
                Trace.Info("NamedPipeClient: connecting to {0}", Name);
                var stream = new NamedPipeClientStream(localhost, Name, PipeDirection.InOut, PipeOptions.Asynchronous);
                Task task = stream.ConnectAsync(cts.Token);
                task.Wait();
                if (task.Status == TaskStatus.Canceled)
                {
                    Trace.Info("NamedPipeClient: cancelled connecting");
                    stream.Close();
                    return;
                }
                Trace.Info("NamedPipeClient: connected to {0}", Name);

                var session = new NamedPipeSession(this, stream);
                OnConnectInternal(session);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            Trace.Info("NamedPipeClient: disposing");

            cts.Cancel();

            cts.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnConnectInternal(LinkSession session)
        {
            base.OnConnectInternal(session);

            session.BeginReceive(true);

            Trace.Info("{0} {1} connected",
                Name, session.InternalHandle);
        }
    }
}

#endif
