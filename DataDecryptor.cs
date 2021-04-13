using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EvilMortyCryptor
{
    class DataDecryptor
    {
        private RSACryptoServiceProvider _provider;
        private RSA _rsa;

        readonly string privateKeyFileName = "private.key";

        public DataDecryptor()
        {
            _rsa = RSA.Create();

            _provider = new RSACryptoServiceProvider();
            if (File.Exists(privateKeyFileName))
            {
                _provider.FromXmlString(File.ReadAllText(privateKeyFileName));
            }
            else
            {
                File.WriteAllText(privateKeyFileName, _provider.ToXmlString(true));
            }
            _rsa.ImportParameters(_provider.ExportParameters(true));
        }

        public string GetPublicKey()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(_provider.ToXmlString(false)));
        }

        //https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.publickey?view=net-5.0
        public string DecryptFromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            byte[] decryptedBytes = null;
            // Create instance of Aes for
            // symetric decryption of the data.
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;

                // Create byte arrays to get the length of
                // the encrypted key and IV.
                // These values were stored as 4 bytes each
                // at the beginning of the encrypted package.
                byte[] LenK = new byte[4];
                byte[] LenIV = new byte[4];


                using (MemoryStream inStream = new MemoryStream(bytes))
                {
                    inStream.Seek(0, SeekOrigin.Begin);
                    inStream.Seek(0, SeekOrigin.Begin);
                    inStream.Read(LenK, 0, 3);
                    inStream.Seek(4, SeekOrigin.Begin);
                    inStream.Read(LenIV, 0, 3);

                    // Convert the lengths to integer values.
                    int lenK = BitConverter.ToInt32(LenK, 0);
                    int lenIV = BitConverter.ToInt32(LenIV, 0);

                    // Determine the start position of
                    // the cipher text (startC)
                    // and its length(lenC).
                    int startC = lenK + lenIV + 8;
                    int lenC = (int)inStream.Length - startC;

                    // Create the byte arrays for
                    // the encrypted Aes key,
                    // the IV, and the cipher text.
                    byte[] KeyEncrypted = new byte[lenK];
                    byte[] IV = new byte[lenIV];

                    // Extract the key and IV
                    // starting from index 8
                    // after the length values.
                    inStream.Seek(8, SeekOrigin.Begin);
                    inStream.Read(KeyEncrypted, 0, lenK);
                    inStream.Seek(8 + lenK, SeekOrigin.Begin);
                    inStream.Read(IV, 0, lenIV);
                    // Use RSA
                    // to decrypt the Aes key.
                    byte[] KeyDecrypted = _rsa.Decrypt(KeyEncrypted, RSAEncryptionPadding.Pkcs1);

                    // Decrypt the key.
                    using (ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV))
                    {

                        // Decrypt the cipher text from
                        // from the FileSteam of the encrypted
                        // file (inFs) into the FileStream
                        // for the decrypted file (outFs).
                        using (MemoryStream outStream = new MemoryStream())
                        {
                            int count = 0;

                            int blockSizeBytes = aes.BlockSize / 8;
                            byte[] data = new byte[blockSizeBytes];

                            // By decrypting a chunk a time,
                            // you can save memory and
                            // accommodate large files.

                            // Start at the beginning
                            // of the cipher text.
                            inStream.Seek(startC, SeekOrigin.Begin);
                            using (CryptoStream outStreamDecrypted = new CryptoStream(outStream, transform, CryptoStreamMode.Write))
                            {
                                do
                                {
                                    count = inStream.Read(data, 0, blockSizeBytes);
                                    outStreamDecrypted.Write(data, 0, count);
                                }
                                while (count > 0);

                                outStreamDecrypted.FlushFinalBlock();
                                outStreamDecrypted.Close();
                            }
                            decryptedBytes = outStream.ToArray();
                        }
                    }
                }
            }
            return Encoding.UTF8.GetString(decryptedBytes);
        }

    }
}
