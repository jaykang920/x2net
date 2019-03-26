// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

#if NET45

using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace x2net
{
    /// <summary>
    /// Named pipe server link.
    /// </summary>
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
            Trace.Info("NamedPipeServer: listening on '{0}'", Name);

            while (!disposed)
            {
                var stream = new NamedPipeServerStream(Name, PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                Task task = stream.WaitForConnectionAsync(cts.Token);
                task.Wait();
                if (task.Status == TaskStatus.Canceled)
                {
                    stream.Close();
                    break;
                }
                Trace.Info("NamedPipeServer: accepted on '{0}'", Name);

                var session = new NamedPipeSession(this, stream);
                OnAcceptInternal(session);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

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
                Trace.Info("{0} {1} accept error: {2}",
                    Name, session.InternalHandle, ex);
                return false;
            }
        }
    }
}

#endif
