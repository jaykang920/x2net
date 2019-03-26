﻿// Copyright (c) 2017-2019 Jae-jun Kang
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
    /// decryption based on the keys exchanged by an asymmetric algorithm.
    /// </summary>
    public sealed class BlockCipher : IBufferTransform
    {
        private SymmetricAlgorithm algorithm;
        private Settings settings;

        private byte[] encryptionKey;
        private byte[] decryptionKey;

        private byte[] encryptionIV;
        private byte[] decryptionIV;

        private int BlockSizeInBytes { get { return (settings.BlockSize >> 3); } }
        private int KeySizeInBytes { get { return (settings.KeySize >> 3); } }

        public int HandshakeBlockLength { get { return (settings.RsaKeySize >> 3); } }

        public BlockCipher()
        {
            settings = new SettingsBuilder {
                BlockSize = 128,
                KeySize = 256,
                RsaKeySize = 1024,

                // In a real-world client/server production, each peer should use a
                // different RSA key pair.
                MyPrivateKey = @"
<RSAKeyValue><Modulus>xtU+mTT9tOES5vLZeSAEvuWaa+FX4jUtH5iVFGSULCaBR6TtQ2TYUz1Jnt
rUhA26OQBIcVzlMyarM8XVhZqk5RJDP64VFz3m+VMmghAgJLUPKDORmIPlc18FuaTsZjxoIwfuVojrDH
/12BoEHHmwb3CVq6dHGsxRLUKG0DYBWQk=</Modulus><Exponent>AQAB</Exponent><P>+3iHfNfD
ARBhnHQ33OyJudsOWkFPwqOG575nkCntjW8RhepXaKPNRqmEu/cYN/Fr/nCmxxgW8Fp5HEI+gI7xZw==
</P><Q>ymoD2gsEj0ksiph+UbkT3Amwx/SHOaRWTwWysL8xKicD0afqnGpHkUnoAUnEQFAnuDIB5D+rb
+6ulwsS6xCsDw==</Q><DP>2d2brJqV1PcnSlAaEepQjFfvwFwzSRM6Ds8UlH7u04k1qkrT/dFkSGMXn
229asJb6O4aYAVL4mLP6J6v3dt54w==</DP><DQ>kLRhtIuT4uupEBwckkgBzpiO7SP/WFIH8c5dBMZq
W3ww2r10mAXSzCdN2T3nMyMagjAd8hMieI7l+c1M5QeyOQ==</DQ><InverseQ>M+sgtHA0blhMUBdGG
IYboxSEvPwPxoX5ORwgL/Zl3TOgxN1oM9i5EkmwKFcazAHKfL5eArtlmfELOcqPMFiyzQ==</InverseQ>
<D>CBEw2AB5ZrRXEv25axusdZ5VNJlQ+oGT0htbuRcXl+78Ac8kPT7DNCVhbkuMocr4ykVDqy3MstW
XzqLxNdl/ZSV9KvP6u5bcDQQeC9KbKQ5PpzGoGmMJNsVtXC0voOA3sYx9P+vVtEqhxn9eAKPOPqX9wRo
9rMW9UZRtDcLiUj0=</D></RSAKeyValue>
",
                PeerPublicKey = @"
<RSAKeyValue><Modulus>xtU+mTT9tOES5vLZeSAEvuWaa+FX4jUtH5iVFGSULCaBR6TtQ2TYUz1Jnt
rUhA26OQBIcVzlMyarM8XVhZqk5RJDP64VFz3m+VMmghAgJLUPKDORmIPlc18FuaTsZjxoIwfuVojrDH
/12BoEHHmwb3CVq6dHGsxRLUKG0DYBWQk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>
"
            }.Build();

            Initialize();
        }

        public BlockCipher(Settings settings)
        {
            this.settings = settings;  // reference-shared

            Initialize();
        }

        private void Initialize()
        {
            algorithm = Aes.Create();
            algorithm.BlockSize = settings.BlockSize;
            algorithm.KeySize = settings.KeySize;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.PKCS7;
        }

        public object Clone()
        {
            return new BlockCipher(settings);
        }

        public void Dispose()
        {
            algorithm.Clear();
        }

        public byte[] InitializeHandshake()
        {
            var challenge = new byte[KeySizeInBytes + BlockSizeInBytes];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(challenge);

            encryptionKey = challenge.SubArray(0, KeySizeInBytes);
            encryptionIV = challenge.SubArray(KeySizeInBytes, BlockSizeInBytes);

            return settings.PeerRsa.Encrypt(challenge, false);
        }

        public byte[] Handshake(byte[] challenge)
        {
            byte[] decrypted;
            decrypted = settings.MyRsa.Decrypt(challenge, false);

            decryptionKey = decrypted.SubArray(0, KeySizeInBytes);
            decryptionIV = decrypted.SubArray(KeySizeInBytes, BlockSizeInBytes);

            // Replay the decrypted data.
            return settings.PeerRsa.Encrypt(decrypted, false);
        }

        public bool FinalizeHandshake(byte[] response)
        {
            byte[] expected = encryptionKey.Concat(encryptionIV);

            // Verify the replayed data.
            byte[] actual = settings.MyRsa.Decrypt(response, false);
            return actual.EqualsEx(expected);
        }

        public int Transform(Buffer buffer, int length)
        {
            Trace.Log("BlockCipher.Transform: input length {0}", length);

            using (var encryptor = algorithm.CreateEncryptor(encryptionKey, encryptionIV))
            using (var ms = new MemoryStream(length + BlockSizeInBytes))
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
#if (UNITY_WORKAROUND && UNITY_MONO)
                // Workaround for ancient mono 2.0 of Unity3D
                // Multiple Write() calls are not properly handled there.

                byte[] plaintext = buffer.ToArray();
                if (Config.TraceLevel <= TraceLevel.Trace)
                {
                    Trace.Log("BlockCipher.Transform: input {0}",
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
                        Trace.Log("BlockCipher.Transform: input block {0}",
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
                    Trace.Log("BlockCipher.Transform: output {0} {1}",
                        result, BitConverter.ToString(streamBuffer, 0, result));
                }

                buffer.Rewind();
                buffer.CopyFrom(streamBuffer, 0, result);

                // Store the last ciphertext block as a next encryption IV.
                System.Buffer.BlockCopy(streamBuffer, result - BlockSizeInBytes,
                    encryptionIV, 0, BlockSizeInBytes);
                        
                return result;
            }
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            Trace.Log("BlockCipher.InverseTransform: input length {0}", length);

            using (var decryptor = algorithm.CreateDecryptor(decryptionKey, decryptionIV))
            using (var ms = new MemoryStream(length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                byte[] nextIV = new byte[BlockSizeInBytes];
#if (UNITY_WORKAROUND && UNITY_MONO)
                // Workaround for ancient mono 2.0 of Unity3D
                // Multiple Write() calls are not properly handled there.

                byte[] ciphertext = buffer.ToArray();
                System.Buffer.BlockCopy(ciphertext, length - BlockSizeInBytes,
                    nextIV, 0, BlockSizeInBytes);
                if (Config.TraceLevel <= TraceLevel.Trace)
                {
                    Trace.Log("BlockCipher.InverseTransform: input {0}",
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
                    int maxToCopy = BlockSizeInBytes - bytesCopied;
                    int bytesToCopy = Math.Min(segment.Count, maxToCopy);
                    System.Buffer.BlockCopy(
                        segment.Array, segment.Offset + segment.Count - bytesToCopy,
                        nextIV, maxToCopy - bytesToCopy,
                        bytesToCopy);
                    bytesCopied += bytesToCopy;
                }

                for (var i = 0; i < buffers.Count; ++i)
                {
                    var segment = buffers[i];

                    if (Config.TraceLevel <= TraceLevel.Trace)
                    {
                        Trace.Log("BlockCipher.InverseTransform: input block {0}",
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
                    Trace.Log("BlockCipher.InverseTransform: output {0} {1}",
                        result, BitConverter.ToString(streamBuffer, 0, result));
                }

                buffer.Rewind();
                buffer.CopyFrom(streamBuffer, 0, result);

                // Store the last ciphertext block as a next decryption IV.
                System.Buffer.BlockCopy(nextIV, 0, decryptionIV, 0, BlockSizeInBytes);

                return result;
            }
        }

        public class SettingsBuilder
        {
            public int BlockSize { get; set; }
            public int KeySize { get; set; }
            public int RsaKeySize { get; set; }

            public string MyPrivateKey { get; set; }
            public string PeerPublicKey { get; set; }

            public Settings Build()
            {
                Settings result = Settings.Create(this);

                var myRsa = new RSACryptoServiceProvider(RsaKeySize);
                var peerRsa = new RSACryptoServiceProvider(RsaKeySize);

                ImportRsaParameters(myRsa, MyPrivateKey);
                ImportRsaParameters(peerRsa, PeerPublicKey);

                result.MyRsa = myRsa;
                result.PeerRsa = peerRsa;

                return result;
            }

            private static void ImportRsaParameters(
                RSACryptoServiceProvider rsa, string xml)
            {
#if NETCORE
                // RSACryptoServiceProvider.FromXmlString workaround for .NET Core 2.0
                RSAParameters parameters = new RSAParameters();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
                {
                    foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
                            case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
                            case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
                            case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
                            case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
                            case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
                            case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
                            case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
                        }
                    }
                }
                else
                {
                    throw new CryptographicException("Invalid XML RSA Parameters");
                }

                rsa.ImportParameters(parameters);
#else
                rsa.FromXmlString(xml);
#endif
            }
        }

        public class Settings : SettingsBuilder
        {
            public RSACryptoServiceProvider MyRsa { get; internal set; }
            public RSACryptoServiceProvider PeerRsa { get; internal set; }

            // Prevent explicit instantiation.
            private Settings() { }

            internal static Settings Create(SettingsBuilder builder)
            {
                return new Settings {
                    BlockSize = builder.BlockSize,
                    KeySize = builder.KeySize,
                    RsaKeySize = builder.RsaKeySize,
                    MyPrivateKey = builder.MyPrivateKey,
                    PeerPublicKey = builder.PeerPublicKey,
                };
            }
        }
    }
}
