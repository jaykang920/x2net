// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace x2net
{
    /// <summary>
    /// A simple example of BufferTransform that performs block encryption and
    /// decryption based on the keys exchanged by an asymmetric algorithm.
    /// </summary>
    /// <remarks>
    /// Illustration purpose only. Do NOT use this as is in production.
    /// </remarks>
    public sealed class BlockCipher : IBufferTransform
    {
        private const int blockSize = 128;
        private const int keySize = 128;
        private const int rsaKeySize = 512;

        private SymmetricAlgorithm encryptionAlgorithm;
        private SymmetricAlgorithm decryptionAlgorithm;

        private byte[] encryptionKey;
        private byte[] decryptionKey;

        private byte[] encryptionIV;
        private byte[] decryptionIV;

        private int BlockSizeInBytes { get { return (blockSize >> 3); } }
        private int KeySizeInBytes { get { return (keySize >> 3); } }

        public int HandshakeBlockLength { get { return (rsaKeySize >> 3); } }

        public BlockCipher()
        {
            encryptionAlgorithm = Aes.Create();
            encryptionAlgorithm.BlockSize = blockSize;
            encryptionAlgorithm.KeySize = keySize;
            encryptionAlgorithm.Mode = CipherMode.CBC;
            encryptionAlgorithm.Padding = PaddingMode.PKCS7;

            decryptionAlgorithm = Aes.Create();
            decryptionAlgorithm.BlockSize = blockSize;
            decryptionAlgorithm.KeySize = keySize;
            decryptionAlgorithm.Mode = CipherMode.CBC;
            decryptionAlgorithm.Padding = PaddingMode.PKCS7;
        }

        public object Clone()
        {
            return new BlockCipher();
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

            using (var rsa = new RSACryptoServiceProvider(rsaKeySize))
            {
                rsa.FromXmlString(rsaPeerPublicKey);
                return rsa.Encrypt(challenge, false);
            }
        }

        public byte[] Handshake(byte[] challenge)
        {
            byte[] decrypted;
            using (var rsa = new RSACryptoServiceProvider(rsaKeySize))
            {
                rsa.FromXmlString(rsaMyPrivateKey);
                decrypted = rsa.Decrypt(challenge, false);

                decryptionKey = decrypted.SubArray(0, KeySizeInBytes);
                decryptionIV = decrypted.SubArray(KeySizeInBytes, BlockSizeInBytes);

                // If we're free from old mono of such as Unity3D,
                // we can simply sign the decrypted data to prove ourselves.
                //return rsa.SignData(decrypted, new SHA1CryptoServiceProvider());
            }
            // But if not, replay the data decrypted with our private key.
            using (var rsa = new RSACryptoServiceProvider(rsaKeySize))
            {
                rsa.FromXmlString(rsaPeerPublicKey);
                return rsa.Encrypt(decrypted, false);
            }
        }

        public bool FinalizeHandshake(byte[] response)
        {
            using (var rsa = new RSACryptoServiceProvider(rsaKeySize))
            {
                byte[] expected = encryptionKey.Concat(encryptionIV);
                
                // If we're free from old mono of such as Unity3D,
                // we can simply verify the peer signature.
                //rsa.FromXmlString(rsaPeerPublicKey);
                //return rsa.VerifyData(expected, new SHA1CryptoServiceProvider(), response);
                
                // But if not, verify the replayed data.
                rsa.FromXmlString(rsaMyPrivateKey);
                byte[] actual = rsa.Decrypt(response, false);
                return actual.EqualsExtended(expected);
            }
        }

        public int Transform(Buffer buffer, int length)
        {
            Log.Trace("BlockCipher.Transform: input length {0}", length);

            using (var encryptor = encryptionAlgorithm.CreateEncryptor(encryptionKey, encryptionIV))
            using (var ms = new MemoryStream(length + BlockSizeInBytes))
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
#if UNITY_WORKAROUND
                // Workaround for ancient mono 2.0 of Unity3D
                // Multiple Write() calls are not properly handled there.

                byte[] plaintext = buffer.ToArray();
                if (Config.LogLevel <= LogLevel.Trace)
                {
                    Log.Trace("BlockCipher.Transform: input {0}",
                        BitConverter.ToString(plaintext, plaintext.Length - length, length));
                }
                cs.Write(plaintext, plaintext.Length - length, length);
#else
                var buffers = new List<ArraySegment<byte>>();
                buffer.ListEndingSegments(buffers, length);

                for (var i = 0; i < buffers.Count; ++i)
                {
                    var segment = buffers[i];

                    if (Config.LogLevel <= LogLevel.Trace)
                    {
                        Log.Trace("BlockCipher.Transform: input block {0}",
                            BitConverter.ToString(segment.Array, segment.Offset, segment.Count));
                    }

                    cs.Write(segment.Array, segment.Offset, segment.Count);
                }
#endif

                cs.FlushFinalBlock();

                int result = (int)ms.Length;
                var streamBuffer = ms.GetBuffer();

                if (Config.LogLevel <= LogLevel.Trace)
                {
                    Log.Trace("BlockCipher.Transform: output {0} {1}",
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
            Log.Trace("BlockCipher.InverseTransform: input length {0}", length);

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
                if (Config.LogLevel <= LogLevel.Trace)
                {
                    Log.Trace("BlockCipher.InverseTransform: input {0}",
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

                    if (Config.LogLevel <= LogLevel.Trace)
                    {
                        Log.Trace("BlockCipher.InverseTransform: input block {0}",
                            BitConverter.ToString(segment.Array, segment.Offset, segment.Count));
                    }

                    cs.Write(segment.Array, segment.Offset, segment.Count);
                }
#endif

                cs.FlushFinalBlock();

                int result = (int)ms.Length;
                var streamBuffer = ms.GetBuffer();

                if (Config.LogLevel <= LogLevel.Trace)
                {
                    Log.Trace("BlockCipher.InverseTransform: output {0} {1}",
                        result, BitConverter.ToString(streamBuffer, 0, result));
                }

                buffer.Rewind();
                buffer.CopyFrom(streamBuffer, 0, result);

                // Store the last ciphertext block as a next decryption IV.
                System.Buffer.BlockCopy(nextIV, 0, decryptionIV, 0, BlockSizeInBytes);

                return result;
            }
        }

        // In a real-world client/server production, each peer should use a
        // different RSA key pair.
        private const string rsaMyPrivateKey = @"
<RSAKeyValue><Modulus>tCNTvJ3bN6uLsqiUMeDGaaUSXyS9bs0m8q2+tmh7QfMwAP9G8CEjFaxyjb
391QeCDsX+lRNf4wsuTJvnbk8rGw==</Modulus><Exponent>AQAB</Exponent><P>8GQnZQd9C4vc
PnezAYD7eTRf01Y52f3/mdhlEi3+1hU=</P><Q>v9Wg0aXwf1TBjnsubTmY9b8ZTnAw2CApHPpUe068+
G8=</Q><DP>Ijj/6sAgKy6kEjiUQViNdHniUoHqBoDEjLBj4yytJOk=</DP><DQ>Q5lOAFKPOu9s/X5e
z9J6Gi7rBf7211IN6s4zsvf+EzU=</DQ><InverseQ>UfiNSsb4iYaAhgbNp3pTFnvwn1uf1sKQoBN7m
Mv0LpA=</InverseQ><D>MzlIen449B+n3enqGjTctvXlv4BnDbbwuFmHvb8ALcRKnY+e5BjF03CSzvK
hDthiOoUk1O9KWo47g0FGaleJIQ==</D></RSAKeyValue>
";
        private const string rsaPeerPublicKey = @"
<RSAKeyValue><Modulus>tCNTvJ3bN6uLsqiUMeDGaaUSXyS9bs0m8q2+tmh7QfMwAP9G8CEjFaxyjb
391QeCDsX+lRNf4wsuTJvnbk8rGw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>
";
    }
}
