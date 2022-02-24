using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace EncryptWithOTP
{
    static class Write
    {
        
        public static void WriteFile(BitArray data, string path)
        {
            byte[] bytes = new byte[data.Length / 8];
            data.CopyTo(bytes, 0);
            File.WriteAllBytes(path, bytes);
        }

        public static void WriteNewFile(BitArray data, string path)
        {
            byte[] bytes = new byte[data.Length / 8];
            data.CopyTo(bytes, 0);
            string[] help = path.Split('\\');

            string newPath = "";
            for (int i = 0; i < help.Length - 1; i++)
            {
                newPath += help[i] + "\\";
            }
            string[] help2 = help[help.Length - 1].Split('.');
            newPath += help2[0] + "-encrypted-copy" + "." + help2[1];

            File.WriteAllBytes(newPath, bytes);
        }
    }
}
