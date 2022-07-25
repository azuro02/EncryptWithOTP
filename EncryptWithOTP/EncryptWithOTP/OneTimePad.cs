using System;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace EncryptWithOTP
{
    static class OneTimePad
    {
        public static OTPvalues Encrypt(List<byte[]> data)
        {
            List<bool[]> dataBits = new List<bool[]>();
            List<byte[]> buffer = new List<byte[]>();
            List<bool[]> key = new List<bool[]>();

            for (int i = 0; i < data.Count; i++)
            {
                dataBits.Add(Converter.GetBools(data[i]));

                using (var generator = RandomNumberGenerator.Create())
                {
                    buffer.Add(new byte[dataBits[i].Length / 8]);
                    generator.GetBytes(buffer[i]);
                }
                key.Add(Converter.GetBools(buffer[i]));

                for (int j = 0; j < buffer[i].Length; j++)
                {
                    buffer[i][j] = 0;
                }

                dataBits[i] = XOR(key[i], dataBits[i]);

                //reset Data
                for (int k = 0; k < data[i].Length; k++)
                {
                    data[i][k] = 0;
                }
            }

            return new OTPvalues(key, dataBits);
        }

        public static List<bool[]> Decrypt(OTPvalues pair)
        {
            for (int i = 0; i < pair.Key.Count; i++)
            {
                pair.CipherText[i] = XOR(pair.CipherText[i], pair.Key[i]);
                for (int j = 0; j < pair.Key[i].Length; j++)
                {
                    pair.Key[i][j] = false;
                }
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
        public List<bool[]> Key { get; set; }
        public List<bool[]> CipherText { get; set; }

        public OTPvalues(List<bool[]> pKey, List<bool[]> pCipherText)
        {
            Key = pKey;
            CipherText = pCipherText;
        }
    }
}
