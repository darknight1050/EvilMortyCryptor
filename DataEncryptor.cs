using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EvilMortyCryptor
{
    class DataEncryptor
    {
        private RSACryptoServiceProvider _provider;
        private RSA _rsa;

        //Set Public Key
        private readonly string _publicKey = "";

        public DataEncryptor()
        {
            _rsa = RSA.Create();
            _provider = new RSACryptoServiceProvider();
            if (HasPublicKey())
                SetPublicKey(_publicKey);
        }

        public bool HasPublicKey()
        {
            return !string.IsNullOrEmpty(_publicKey);
        }

        public void SetPublicKey(string publicKey)
        {
            _provider.FromXmlString(Encoding.UTF8.GetString(Convert.FromBase64String(publicKey)));
            _rsa.ImportParameters(_provider.ExportParameters(false));
        }

        //https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.publickey?view=net-5.0
        public string EncryptToBase64(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes;
            using (Aes aes = Aes.Create())
            {
                // Create instance of Aes for
                // symetric encryption of the data.
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                using (ICryptoTransform transform = aes.CreateEncryptor())
                {
                    RSAPKCS1KeyExchangeFormatter keyFormatter = new RSAPKCS1KeyExchangeFormatter(_rsa);
                    byte[] keyEncrypted = keyFormatter.CreateKeyExchange(aes.Key, aes.GetType());

                    // Create byte arrays to contain
                    // the length values of the key and IV.
                    byte[] LenK = new byte[4];
                    byte[] LenIV = new byte[4];

                    int lKey = keyEncrypted.Length;
                    LenK = BitConverter.GetBytes(lKey);
                    int lIV = aes.IV.Length;
                    LenIV = BitConverter.GetBytes(lIV);

                    // Write the following to the MemoryStream
                    // for the encrypted file (outStream):
                    // - length of the key
                    // - length of the IV
                    // - ecrypted key
                    // - the IV
                    // - the encrypted cipher content
                    using (MemoryStream outStream = new MemoryStream())
                    {

                        outStream.Write(LenK, 0, 4);
                        outStream.Write(LenIV, 0, 4);
                        outStream.Write(keyEncrypted, 0, lKey);
                        outStream.Write(aes.IV, 0, lIV);

                        // Now write the cipher text using
                        // a CryptoStream for encrypting.
                        using (CryptoStream outStreamEncrypted = new CryptoStream(outStream, transform, CryptoStreamMode.Write))
                        {

                            outStreamEncrypted.Write(bytes, 0, bytes.Length);
                            outStreamEncrypted.FlushFinalBlock();
                            outStreamEncrypted.Close();
                        }
                        encryptedBytes = outStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encryptedBytes);
        }

    }
}
