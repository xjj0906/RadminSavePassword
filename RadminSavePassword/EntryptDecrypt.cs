using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RadminSavePassword
{
    public class EntryptDecrypt
    {
        const string Key = "~x1o5*7%";//8位key

        public string Encrypt(string pToEncrypt)
        {
            if (string.IsNullOrEmpty(pToEncrypt))
                return string.Empty;
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(Key);
                byte[] rgbIV = Encoding.UTF8.GetBytes(Key);
                byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                DESCryptoServiceProvider dcsp = new DESCryptoServiceProvider();
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, dcsp.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(inputByteArray, 0, inputByteArray.Length);
                        cStream.FlushFinalBlock();
                        return Convert.ToBase64String(mStream.ToArray());
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public string Decrypt(string pToDecrypt)
        {
            if (string.IsNullOrEmpty(pToDecrypt))
                return string.Empty;
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(Key);
                byte[] rgbIV = Encoding.UTF8.GetBytes(Key);
                byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
                DESCryptoServiceProvider dcsp = new DESCryptoServiceProvider();
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, dcsp.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(inputByteArray, 0, inputByteArray.Length);
                        cStream.FlushFinalBlock();
                        return Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        } 
    }
}