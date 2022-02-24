using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EncryptWithOTP
{
    static class OneTimePad
    {
        public static OTPvalues Encrypt(byte[] data)
        {
            BitArray dataBits = new BitArray(data);

            var bytes = new byte[data.Length];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes); 
            }
            BitArray key = new BitArray(bytes);
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
            pair.cipherText.Xor(pair.key);
            for (int i = 0; i < pair.key.Length; i++)
            {
                pair.key[i] = false;
            }
            return pair.cipherText;
        }
    }



    struct OTPvalues
    {
        public BitArray key { get; set; }
        public BitArray cipherText { get; }

        public OTPvalues(BitArray pKey, BitArray pCipherText)
        {
            key = pKey;
            cipherText = pCipherText;
        }

    }
}
