using System;
using System.IO;
using System.Security.Cryptography;


namespace ConsoleApplication1
{
    public class Encryption
    {
        // Change these keys
        private byte[] Key = { 123, 217, 19, 11, 24, 26, 85, 45, 114, 184, 27, 162, 37, 112, 222, 209, 241, 24, 175, 144, 173, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };
        private byte[] Vector = { 146, 64, 191, 111, 23, 3, 113, 119, 231, 121, 252, 112, 79, 32, 114, 156 };

        private ICryptoTransform EncryptorTransform, DecryptorTransform;
        private System.Text.UTF8Encoding UTFEncoder;

        public Encryption()
        {
            RijndaelManaged rm = new RijndaelManaged();

            EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
            DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

            UTFEncoder = new System.Text.UTF8Encoding();
        }

        /// Generates encryption key.
        static public byte[] GenerateEncryptionKey()
        {
            //Generate a Key.
            RijndaelManaged rm = new RijndaelManaged();
            rm.GenerateKey();
            return rm.Key;
        }

        /// Generates encryption vector
        static public byte[] GenerateEncryptionVector()
        {
            RijndaelManaged rm = new RijndaelManaged();
            rm.GenerateIV();
            return rm.IV;
        }

        public string EncryptToString(string TextValue)
        {
            return ByteArrToString(Encrypt(TextValue));
        }

        public byte[] Encrypt(string TextValue)
        {
            Byte[] bytes = UTFEncoder.GetBytes(TextValue);

            MemoryStream memoryStream = new MemoryStream();

            CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();

            memoryStream.Position = 0;
            byte[] encrypted = new byte[memoryStream.Length];
            memoryStream.Read(encrypted, 0, encrypted.Length);

            cs.Close();
            memoryStream.Close();

            return encrypted;
        }

        /// The other side: Decryption methods
        public string DecryptString(string EncryptedString)
        {
            return Decrypt(StrToByteArray(EncryptedString));
        }

        public string Decrypt(byte[] EncryptedValue)
        {
            MemoryStream encryptedStream = new MemoryStream();
            CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
            decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
            decryptStream.FlushFinalBlock();

            encryptedStream.Position = 0;
            Byte[] decryptedBytes = new Byte[encryptedStream.Length];
            encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
            encryptedStream.Close();
            return UTFEncoder.GetString(decryptedBytes);
        }

        public byte[] StrToByteArray(string str)
        {
            if (str.Length == 0)
                throw new Exception("Invalid string value in StrToByteArray");

            byte val;
            byte[] byteArr = new byte[str.Length / 3];
            int i = 0;
            int j = 0;
            do
            {
                val = byte.Parse(str.Substring(i, 3));
                byteArr[j++] = val;
                i += 3;
            }
            while (i < str.Length);
            return byteArr;
        }

        public string ByteArrToString(byte[] byteArr)
        {
            byte val;
            string tempStr = "";
            for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
            {
                val = byteArr[i];
                if (val < (byte)10)
                    tempStr += "00" + val.ToString();
                else if (val < (byte)100)
                    tempStr += "0" + val.ToString();
                else
                    tempStr += val.ToString();
            }
            return tempStr;
        }
    }
}
