// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace x2net
{
    public class NamedPipeServer : ServerLink
    {
        CancellationTokenSource cts;

        public NamedPipeServer(string name) : base(name)
        {
            cts = new CancellationTokenSource();
        }

        public void Listen()
        {
            Task.Run((Action)Accept);
        }

        void Accept()
        {
            Trace.Info("NamedPipeServer: listening on {0}", Name);
            while (!disposed)
            {
                Trace.Info("NamedServerPipe: accept loop");

                var stream = new NamedPipeServerStream(Name, PipeDirection.InOut, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                Task task = stream.WaitForConnectionAsync(cts.Token);
                task.Wait();
                Trace.Warn("NamedServerPipe: task status {0}", task.Status);
                if (task.Status == TaskStatus.Canceled)
                {
                    Trace.Info("NamedServerPipe: cancelled listening");
                    stream.Close();
                    break;
                }
                // hendle
                Trace.Info("NamedServerPipe: accepted");
                var session = new NamedPipeSession(this, stream);
                OnAcceptInternal(session);
            }
            // hendle
            Trace.Info("NamedServerPipe: exit listening");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            Trace.Info("NamedPipeServer: disposing");

            cts.Cancel();

            cts.Dispose();

            base.Dispose(disposing);
        }

        protected override bool OnAcceptInternal(LinkSession session)
        {
            try
            {
                if (!base.OnAcceptInternal(session))
                {
                    return false;
                }

                session.BeginReceive(true);

                Trace.Info("{0} {1} accepted",
                    Name, session.InternalHandle);

                return true;
            }
            catch (ObjectDisposedException)
            {
                Trace.Log("{0} {1} accept error: closed immediately",
                    Name, session.InternalHandle);
                return false;
            }
            catch (Exception ex)
            {
                Trace.Warn("{0} {1} accept error: {2}",
                    Name, session.InternalHandle, ex);
                return false;
            }
        }
    }
}
