using System;
using System.Collections;
using System.Security.Cryptography;


namespace EncryptWithOTP
{
    static class OneTimePad
    {
        public static OTPvalues Encrypt(byte[] data)
        {
            bool[] dataBits = new bool[data.Length * 8];//Error bei Dateien > 512mb
            dataBits = Converter.GetBools(data);

            byte[] bytes = new byte[data.Length];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }
            bool[] key = new bool[bytes.Length];
            key = Converter.GetBools(bytes);

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }

            dataBits = XOR(key, dataBits);

            //reset Data
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }

            return new OTPvalues(key, dataBits);
        }

        public static bool[] Decrypt(OTPvalues pair)
        {
            pair.CipherText = XOR(pair.CipherText, pair.Key);
            for (int i = 0; i < pair.Key.Length; i++)
            {
                pair.Key[i] = false;
            }
            return pair.CipherText;
        }

        public static bool[] XOR(bool[] a, bool[] b)
        {
            bool[] ergebnis = new bool[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                ergebnis[i] = a[i] ^ b[i];
            }
            return ergebnis;
        }
    }

    struct OTPvalues
    {
        public bool[] Key { get; set; }
        public bool[] CipherText { get; set; }

        public OTPvalues(bool[] pKey, bool[] pCipherText)
        {
            Key = pKey;
            CipherText = pCipherText;
        }
    }
}
