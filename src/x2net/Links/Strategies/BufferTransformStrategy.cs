// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Threading;

namespace x2net
{
    public class BufferTransformStrategy : ChannelStrategy
    {
        /// <summary>
        /// Gets or sets the BufferTransform for the associated link.
        /// </summary>
        public IBufferTransform BufferTransform { get; set; }

        static BufferTransformStrategy()
        {
            EventFactory.Global.Register(HandshakeReq.TypeId, HandshakeReq.New);
            EventFactory.Global.Register(HandshakeResp.TypeId, HandshakeResp.New);
            EventFactory.Global.Register(HandshakeAck.TypeId, HandshakeAck.New);
        }

        public override void Setup()
        {
            base.Setup();
        }

        public override void BeforeSessionSetup(LinkSession linkSession)
        {
            linkSession.ChannelStrategy = new BufferTransformSessionStrategy {
                Session = linkSession
            };
        }

        public override void InitiateHandshake(LinkSession session)
        {
            if (ReferenceEquals(BufferTransform, null))
            {
                return;
            }

            var sessionStrategy =
                (BufferTransformSessionStrategy)session.ChannelStrategy;

            var bufferTransform = (IBufferTransform)BufferTransform.Clone();
            sessionStrategy.BufferTransform = bufferTransform;
            WaitSignalPool.Acquire(session.InternalHandle).Set();

            session.Send(new HandshakeReq {
                _Transform = false,
                Data = bufferTransform.InitializeHandshake()
            });
        }

        public override void Release()
        {
            if (ReferenceEquals(BufferTransform, null))
            {
                return;
            }
            BufferTransform.Dispose();
            BufferTransform = null;
        }
    }

    public class BufferTransformSessionStrategy : ChannelStrategy.SubStrategy
    {
        private volatile bool rxTransformReady;
        private volatile bool txTransformReady;

        /// <summary>
        /// Gets or sets the BufferTransform for the associated link session.
        /// </summary>
        public IBufferTransform BufferTransform { get; set; }

        public bool RxTransformReady
        {
            get { return rxTransformReady; }
            set { rxTransformReady = value; }
        }
        public bool TxTransformReady
        {
            get { return txTransformReady; }
            set { txTransformReady = value; }
        }

        public override bool Process(Event e)
        {
            switch (e.GetTypeId())
            {
                case (int)LinkEventType.HandshakeReq:
                    {
                        var req = (HandshakeReq)e;
                        var resp = new HandshakeResp { _Transform = false };
                        byte[] response = null;
                        try
                        {
                            ManualResetEvent waitHandle =
                                WaitSignalPool.Acquire(Session.InternalHandle);
                            waitHandle.WaitOne(new TimeSpan(0, 0, 30));
                            WaitSignalPool.Release(Session.InternalHandle);
                            response = BufferTransform.Handshake(req.Data);
                        }
                        catch (Exception ex)
                        {
                            Trace.Error("{0} {1} error handshaking : {2}",
                                Session.Link.Name, Session.InternalHandle, ex.ToString());
                        }
                        if (response != null)
                        {
                            resp.Data = response;
                        }
                        Session.Send(resp);
                    }
                    break;
                case (int)LinkEventType.HandshakeResp:
                    {
                        var ack = new HandshakeAck { _Transform = false };
                        var resp = (HandshakeResp)e;
                        try
                        {
                            if (BufferTransform.FinalizeHandshake(resp.Data))
                            {
                                rxTransformReady = true;
                                ack.Result = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.Error("{0} {1} error finishing handshake : {2}",
                                Session.Link.Name, Session.InternalHandle, ex.ToString());
                        }
                        Session.Send(ack);
                    }
                    break;
                case (int)LinkEventType.HandshakeAck:
                    {
                        var ack = (HandshakeAck)e;
                        bool result = ack.Result;

                        if (result)
                        {
                            txTransformReady = true;
                        }

                        Session.Link.OnLinkSessionConnectedInternal(result, (result ? Session: null));
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override void Release()
        {
            if (ReferenceEquals(BufferTransform, null))
            {
                return;
            }
            BufferTransform.Dispose();
            BufferTransform = null;
        }

        public override bool BeforeSend(Buffer buffer, int length)
        {
            if (txTransformReady)
            {
                BufferTransform.Transform(buffer, length);
                return true;
            }
            return false;
        }

        public override void AfterReceive(Buffer buffer, int length)
        {
            if (rxTransformReady)
            {
                BufferTransform.InverseTransform(buffer, length);
            }
        }
    }
}
