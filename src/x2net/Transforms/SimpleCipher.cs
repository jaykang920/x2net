// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
#if NETCORE
using System.Xml;
#endif

namespace x2net
{
    /// <summary>
    /// A simple example of BufferTransform that performs block encryption and
    /// decryption based on the predefined key.
    /// </summary>
    public sealed class SimpleCipher : IBufferTransform
    {
        private SymmetricAlgorithm algorithm;
        private Settings settings;

        private int BlockSizeInBytes { get { return (settings.BlockSize >> 3); } }
        private int KeySizeInBytes { get { return (settings.KeySize >> 3); } }

        public int HandshakeBlockLength { get { return 0; } }

        public SimpleCipher()
        {
            settings = new Settings {
                BlockSize = 128,
                KeySize = 256,

                IV = new byte[] {
                    (byte)0x4f, (byte)0x2f, (byte)0x4b, (byte)0x42,
                    (byte)0xe6, (byte)0xc8, (byte)0x0a, (byte)0x2f,
                    (byte)0xae, (byte)0x6e, (byte)0xbc, (byte)0x82,
                    (byte)0x59, (byte)0x51, (byte)0x36, (byte)0x69
                },
                Key = new byte[] {
                    (byte)0x02, (byte)0x4d, (byte)0xeb, (byte)0x41,
                    (byte)0x79, (byte)0xc5, (byte)0x2f, (byte)0xd3,
                    (byte)0xa3, (byte)0x61, (byte)0xcc, (byte)0x37,
                    (byte)0x95, (byte)0x94, (byte)0xc2, (byte)0x37,
                    (byte)0x24, (byte)0xa9, (byte)0x59, (byte)0x6a,
                    (byte)0xae, (byte)0xb8, (byte)0x26, (byte)0xbf,
                    (byte)0xd0, (byte)0x19, (byte)0x65, (byte)0x3f,
                    (byte)0x61, (byte)0x35, (byte)0x9e, (byte)0x86
                }
            };

            Initialize();
        }

        public SimpleCipher(Settings settings)
        {
            this.settings = settings;

            Initialize();
        }

        private void Initialize()
        {
            algorithm = Aes.Create();
            algorithm.BlockSize = settings.BlockSize;
            algorithm.KeySize = settings.KeySize;
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.PKCS7;
        }

        public object Clone()
        {
            return new SimpleCipher(settings);
        }

        public void Dispose()
        {
            algorithm.Clear();
        }

        public byte[] InitializeHandshake()
        {
            return null;
        }

        public byte[] Handshake(byte[] challenge)
        {
            return null;
        }

        public bool FinalizeHandshake(byte[] response)
        {
            return true;
        }

        public int Transform(Buffer buffer, int length)
        {
            Trace.Log("SimpleCipher.Transform: input length {0}", length);

            using (var encryptor = algorithm.CreateEncryptor(settings.Key, settings.IV))
            using (var ms = new MemoryStream(length + BlockSizeInBytes))
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
#if UNITY_WORKAROUND
                // Workaround for ancient mono 2.0 of Unity3D
                // Multiple Write() calls are not properly handled there.

                byte[] plaintext = buffer.ToArray();
                if (Config.TraceLevel <= TraceLevel.Trace)
                {
                    Trace.Log("SimpleCipher.Transform: input {0}",
                        BitConverter.ToString(plaintext, plaintext.Length - length, length));
                }
                cs.Write(plaintext, plaintext.Length - length, length);
#else
                var buffers = new List<ArraySegment<byte>>();
                buffer.ListEndingSegments(buffers, length);

                for (var i = 0; i < buffers.Count; ++i)
                {
                    var segment = buffers[i];

                    if (Config.TraceLevel <= TraceLevel.Trace)
                    {
                        Trace.Log("SimpleCipher.Transform: input block {0}",
                            BitConverter.ToString(segment.Array, segment.Offset, segment.Count));
                    }

                    cs.Write(segment.Array, segment.Offset, segment.Count);
                }
#endif

                cs.FlushFinalBlock();

                int result = (int)ms.Length;
                var streamBuffer = ms.GetBuffer();

                if (Config.TraceLevel <= TraceLevel.Trace)
                {
                    Trace.Log("SimpleCipher.Transform: output {0} {1}",
                        result, BitConverter.ToString(streamBuffer, 0, result));
                }

                buffer.Rewind();
                buffer.CopyFrom(streamBuffer, 0, result);
                        
                return result;
            }
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            Trace.Log("SimpleCipher.InverseTransform: input length {0}", length);

            using (var decryptor = algorithm.CreateDecryptor(settings.Key, settings.IV))
            using (var ms = new MemoryStream(length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                byte[] nextIV = new byte[BlockSizeInBytes];
#if UNITY_WORKAROUND
                // Workaround for ancient mono 2.0 of Unity3D
                // Multiple Write() calls are not properly handled there.

                byte[] ciphertext = buffer.ToArray();
                System.Buffer.BlockCopy(ciphertext, length - BlockSizeInBytes,
                    nextIV, 0, BlockSizeInBytes);
                if (Config.TraceLevel <= TraceLevel.Trace)
                {
                    Trace.Log("SimpleCipher.InverseTransform: input {0}",
                        BitConverter.ToString(ciphertext, 0, length));
                }
                cs.Write(ciphertext, 0, length);
#else
                var buffers = new List<ArraySegment<byte>>();
                buffer.ListStartingSegments(buffers, length);

                // Capture the last ciphertext block.
                int bytesCopied = 0;
                for (var i = buffers.Count - 1; bytesCopied < BlockSizeInBytes && i >= 0; --i)
                {
                    var segment = buffers[i];
                    int bytesToCopy = Math.Min(segment.Count, BlockSizeInBytes);
                    System.Buffer.BlockCopy(segment.Array, segment.Offset + segment.Count - bytesToCopy,
                        nextIV, BlockSizeInBytes - bytesCopied - bytesToCopy, bytesToCopy);
                    bytesCopied += bytesToCopy;
                }

                for (var i = 0; i < buffers.Count; ++i)
                {
                    var segment = buffers[i];

                    if (Config.TraceLevel <= TraceLevel.Trace)
                    {
                        Trace.Log("SimpleCipher.InverseTransform: input block {0}",
                            BitConverter.ToString(segment.Array, segment.Offset, segment.Count));
                    }

                    cs.Write(segment.Array, segment.Offset, segment.Count);
                }
#endif

                cs.FlushFinalBlock();

                int result = (int)ms.Length;
                var streamBuffer = ms.GetBuffer();

                if (Config.TraceLevel <= TraceLevel.Trace)
                {
                    Trace.Log("SimpleCipher.InverseTransform: output {0} {1}",
                        result, BitConverter.ToString(streamBuffer, 0, result));
                }

                buffer.Rewind();
                buffer.CopyFrom(streamBuffer, 0, result);

                return result;
            }
        }

        public class Settings
        {
            public int BlockSize { get; set; }
            public int KeySize { get; set; }

            public byte[] IV { get; set; }
            public byte[] Key { get; set; }
        }
    }
}
