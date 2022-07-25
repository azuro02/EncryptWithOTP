using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Collections.Generic;


namespace EncryptWithOTP
{
    static class Write
    {
        public static void WriteFile(bool[] data, string path)
        {
            byte[] dataByte = new byte[data.Length / 8];
            dataByte = Converter.GetBytes(data);
            File.WriteAllBytes(path, dataByte);
        }

        public static void WriteFile(List<bool[]> dataList, string path)
        {
           
            List<byte[]> dataByte = new List<byte[]>();
            dataByte.Add(new byte[dataList[0].Length / 8]);
            dataByte[0] = Converter.GetBytes(dataList[0]);
            File.WriteAllBytes(path, dataByte[0]); //Datei wird erstellt

            for (int i = 1; i < dataList.Count; i++)
            {
                dataByte.Add(new byte[dataList[0].Length / 8]);
                AppendAllBytes(path, dataByte[i]);
            }
        }

        public static void WriteNewFile(byte[] data, string path)
        {
            string[] help = path.Split('\\');

            string newPath = "";
            for (int i = 0; i < help.Length - 1; i++)
            {
                newPath += help[i] + "\\";
            }
            string[] help2 = help[help.Length - 1].Split('.');
            newPath += help2[0] + "-encrypted-copy" + "." + help2[1];

            File.WriteAllBytes(newPath, data);
        }

        public static void WriteNewFile(List<byte[]> byteList, string path)
        {
            string[] help = path.Split('\\');

            string newPath = "";
            for (int i = 0; i < help.Length - 1; i++)
            {
                newPath += help[i] + "\\";
            }
            string[] help2 = help[help.Length - 1].Split('.');
            newPath += help2[0] + "-encrypted-copy" + "." + help2[1];

            File.WriteAllBytes(newPath, byteList[0]);

            for (int i = 1; i < byteList.Count; i++)
            {
                AppendAllBytes(path, byteList[i]);
            }
        }

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
