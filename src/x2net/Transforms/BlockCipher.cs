// Copyright (c) 2017 Jae-jun Kang
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
        private Settings settings;

        private SymmetricAlgorithm encryptionAlgorithm;
        private SymmetricAlgorithm decryptionAlgorithm;

        private byte[] encryptionKey;
        private byte[] decryptionKey;

        private byte[] encryptionIV;
        private byte[] decryptionIV;

        private int BlockSizeInBytes { get { return (settings.BlockSize >> 3); } }
        private int KeySizeInBytes { get { return (settings.KeySize >> 3); } }

        public int HandshakeBlockLength { get { return (settings.RsaKeySize >> 3); } }

        public BlockCipher()
        {
            settings = new Settings {
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
            };

            Initialize();
        }

        public BlockCipher(Settings settings)
        {
            this.settings = settings;

            Initialize();
        }

        private void Initialize()
        {
            encryptionAlgorithm = Aes.Create();
            encryptionAlgorithm.BlockSize = settings.BlockSize;
            encryptionAlgorithm.KeySize = settings.KeySize;
            encryptionAlgorithm.Mode = CipherMode.CBC;
            encryptionAlgorithm.Padding = PaddingMode.PKCS7;

            decryptionAlgorithm = Aes.Create();
            decryptionAlgorithm.BlockSize = settings.BlockSize;
            decryptionAlgorithm.KeySize = settings.KeySize;
            decryptionAlgorithm.Mode = CipherMode.CBC;
            decryptionAlgorithm.Padding = PaddingMode.PKCS7;
        }

        public object Clone()
        {
            return new BlockCipher(settings);
        }

        public void Dispose()
        {
            encryptionAlgorithm.Clear();
            decryptionAlgorithm.Clear();
        }

        public byte[] InitializeHandshake()
        {
            var challenge = new byte[KeySizeInBytes + BlockSizeInBytes];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(challenge);

            encryptionKey = challenge.SubArray(0, KeySizeInBytes);
            encryptionIV = challenge.SubArray(KeySizeInBytes, BlockSizeInBytes);

            using (var rsa = new RSACryptoServiceProvider(settings.RsaKeySize))
            {
                ImportRsaParameters(rsa, settings.PeerPublicKey);
                return rsa.Encrypt(challenge, false);
            }
        }

        public byte[] Handshake(byte[] challenge)
        {
            byte[] decrypted;
            using (var rsa = new RSACryptoServiceProvider(settings.RsaKeySize))
            {
                ImportRsaParameters(rsa, settings.MyPrivateKey);
                decrypted = rsa.Decrypt(challenge, false);

                decryptionKey = decrypted.SubArray(0, KeySizeInBytes);
                decryptionIV = decrypted.SubArray(KeySizeInBytes, BlockSizeInBytes);

                // If we're free from old mono of such as Unity3D,
                // we can simply sign the decrypted data to prove ourselves.
                //return rsa.SignData(decrypted, new SHA1CryptoServiceProvider());
            }
            // But if not, replay the data decrypted with our private key.
            using (var rsa = new RSACryptoServiceProvider(settings.RsaKeySize))
            {
                ImportRsaParameters(rsa, settings.PeerPublicKey);
                return rsa.Encrypt(decrypted, false);
            }
        }

        public bool FinalizeHandshake(byte[] response)
        {
            using (var rsa = new RSACryptoServiceProvider(settings.RsaKeySize))
            {
                byte[] expected = encryptionKey.Concat(encryptionIV);

                // If we're free from old mono of such as Unity3D,
                // we can simply verify the peer signature.
                //rsa.FromXmlString(rsaPeerPublicKey);
                //return rsa.VerifyData(expected, new SHA1CryptoServiceProvider(), response);

                // But if not, verify the replayed data.
                ImportRsaParameters(rsa, settings.MyPrivateKey);
                byte[] actual = rsa.Decrypt(response, false);
                return actual.EqualsExtended(expected);
            }
        }

        public int Transform(Buffer buffer, int length)
        {
            Trace.Log("BlockCipher.Transform: input length {0}", length);

            using (var encryptor = encryptionAlgorithm.CreateEncryptor(encryptionKey, encryptionIV))
            using (var ms = new MemoryStream(length + BlockSizeInBytes))
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
#if UNITY_WORKAROUND
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

            using (var decryptor = decryptionAlgorithm.CreateDecryptor(decryptionKey, decryptionIV))
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

        private void ImportRsaParameters(RSACryptoServiceProvider rsa, string xml)
        {
#if NETCORE
            // RSACryptoServiceProvider.FromXmlString workaround for .NET Core 2.0 preview
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

        public class Settings
        {
            public int BlockSize { get; set; }
            public int KeySize { get; set; }
            public int RsaKeySize { get; set; }

            public string MyPrivateKey { get; set; }
            public string PeerPublicKey { get; set; }
        }
    }
}
