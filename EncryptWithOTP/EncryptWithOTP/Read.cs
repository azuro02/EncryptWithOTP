using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace EncryptWithOTP
{
    class Read
    {
        public List<byte[]> Result { get; set; }

        int folderIndex;
        bool firstIteration = true;
        public List<string> FailedPaths { get; set; }
        int progressIndex = 0;

        public Read(string path)
        {
            Result = new List<byte[]>();
            FailedPaths = new List<string>();

            string[] help = path.Split('\\');
            folderIndex = help.Length - 1;
        }

        public List<byte[]> ReadLargeFile(string path)
        {
            List<byte[]> bytes = new List<byte[]>();

            byte[] buffer = new byte[2147483591]; //MAX Items in one Array
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                for (int i = 0; i < fs.Length; i += 2147483591)
                {
                    fs.Read(buffer, i, 2147483591);
                    bytes.Add(buffer);
                }
            }
            return bytes;
        }


        public byte[] ReadAndEncryptFile(string path, string name, string keyDirPath)
        {
            List<byte[]> byteList = ReadLargeFile(path);

            var bytes = File.ReadAllBytes(path); //Read in File


            OTPvalues pair = OneTimePad.Encrypt(bytes);//Encrypt

            Write.WriteFile(pair.Key, keyDirPath + @"\" + name + "_key.txt");//Save Keys

            pair.Key = null;

            try
            {
                Write.WriteFile(pair.CipherText, path); //create new ciphered File
            }
            catch
            {
                FailedPaths.Add(path);
                byte[] by = new byte[pair.CipherText.Length / 8];
                by = Converter.GetBytes(pair.CipherText);
                Write.WriteNewFile(by, path);
            }

            return bytes;
        }

        public void ReadAndEncryptDirectory(string path, IProgress<int> progress, int count, string keypath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            string currentKeyPath = "";
            if (keypath == "")
            {
                string[] help = path.Split('\\');
                help[folderIndex] += "_keys";


                for (int i = 0; i < help.Length; i++)
                {
                    currentKeyPath += help[i];
                    if (i != help.Length - 1)
                    {
                        currentKeyPath += "\\";
                    }
                }
            }
            else
            {
                string[] help = path.Split('\\');
                help[folderIndex] += "_keys";
                currentKeyPath = keypath;


                for (int i = folderIndex; i < help.Length; i++)
                {
                    currentKeyPath += "\\" + help[i];
                }
            }

            DirectoryInfo keyDir = new DirectoryInfo(currentKeyPath);
            if (firstIteration)
            {
                firstIteration = false;
                if (!keyDir.Exists)
                {
                    keyDir.Create();
                }
                else
                {
                    MessageBoxButton buttons = MessageBoxButton.YesNo;
                    var result = MessageBox.Show("A key folder with this name alresy exists. Do you want to override the old folder?" + System.Environment.NewLine + "! You may lose youre keys. This cannot be undone !", "override", buttons);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            }

            foreach (var file in dirInfo.GetFiles())
            {
                ReadAndEncryptFile(file.FullName, file.Name, keyDir.FullName);
                progressIndex++;
                var percentageComplete = (progressIndex * 100) / count;
                progress.Report(percentageComplete);
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                DirectoryInfo keyDir2 = new DirectoryInfo(currentKeyPath);
                keyDir = new DirectoryInfo(keyDir2.FullName + "\\" + dir.Name);
                keyDir.Create();
                ReadAndEncryptDirectory(dir.FullName, progress, count, keypath);
            }
        }

        public List<byte[]> ReadDirectory(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.GetFiles())
            {
                Result.Add(ReadFile(file.FullName));
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                ReadDirectory(dir.FullName);
            }
            return Result;
        }

        public byte[] ReadFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            return bytes;
        }
    }
}
