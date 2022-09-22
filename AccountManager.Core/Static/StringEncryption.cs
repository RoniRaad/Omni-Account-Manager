using System.Security.Cryptography;
using System.Text;

namespace AccountManager.Core.Static
{
    public static class StringEncryption
    {
        private static readonly int KeySize = 32; // 256 bit
        private static readonly byte[] Salt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        public static string EncryptString(string key, string plainText)
        {
            byte[] iv;
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV();
                iv = aes.IV;
                aes.Key = Convert.FromBase64String(key);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array.Concat(iv).ToArray());
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);
            byte[] iv = buffer[^16..];
            buffer = buffer[..^16];

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string Hash(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(
              password,
              Salt,
              1000,
              HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));

                return key;
            }
        }

        public static string DecryptEpicGamesData(string toDecrypt, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            PadToMultipleOf(ref keyArray, 8);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            Aes rDel = Aes.Create();
            rDel.KeySize = (keyArray.Length * 8);
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7; 
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static string EncryptEpicGamesData(string toEncrypt, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            PadToMultipleOf(ref keyArray, 8);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            Aes rDel = Aes.Create();
            rDel.KeySize = (keyArray.Length * 8);
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }

        private static void PadToMultipleOf(ref byte[] src, int pad)
        {
            int len = (src.Length + pad - 1) / pad * pad;
            Array.Resize(ref src, len);
        }
    }
}