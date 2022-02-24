using System;
using System.Collections;
using System.Security.Cryptography;


namespace EncryptWithOTP
{
    static class OneTimePad
    {
        public static OTPvalues Encrypt(byte[] data)
        {
            BitArray dataBits = new BitArray(data);

            byte[] bytes = new byte[data.Length];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes); 
            }
            BitArray key = new BitArray(bytes);
            
            for(int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }
            
            dataBits.Xor(key);

            //reset Data
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }
            return new OTPvalues(key, dataBits);
        }

        public static BitArray Decrypt(OTPvalues pair)
        {
            pair.CipherText.Xor(pair.Key);
            for (int i = 0; i < pair.Key.Length; i++)
            {
                pair.Key[i] = false;
            }
            return pair.CipherText;
        }
    }



    struct OTPvalues
    {
        public BitArray? Key { get; set; }
        public BitArray CipherText { get; }

        public OTPvalues(BitArray pKey, BitArray pCipherText)
        {
            Key = pKey;
            CipherText = pCipherText;
        }

    }
}
