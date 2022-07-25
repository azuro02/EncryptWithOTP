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

            int maxArrayItems = 3000 / 8; //MAX Items in one Array (2147483591) / 8 so a bool array can include all data

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                if(fs.Length < maxArrayItems)
                {
                    byte[] buffer = new byte[fs.Length];
                    for (int i = 0; i < fs.Length; i += maxArrayItems) //for-Schleife evt unnötig
                    {
                        fs.Read(buffer, i,(int)fs.Length); //Achtung Konvertierung Prüfen
                        bytes.Add(buffer);
                    }
                }
                else
                {
                    long remainingLength = fs.Length;
                    byte[] buffer = new byte[maxArrayItems];
                    for (int i = 0; i < fs.Length; i += maxArrayItems)
                    {
                        if(remainingLength > maxArrayItems)
                        {
                            buffer = new byte[maxArrayItems];
                            fs.Read(buffer, 0, maxArrayItems);
                            bytes.Add(buffer);
                            remainingLength -= maxArrayItems;
                        }
                        else
                        {
                            buffer = new byte[(int)remainingLength];
                            fs.Read(buffer, 0, (int)remainingLength); //Achtung Konvertierung Prüfen
                            bytes.Add(buffer);
                        }
                    }
                }
            }
            return bytes;
        }


        public List<byte[]> ReadAndEncryptFile(string path, string name, string keyDirPath)
        {
            List<byte[]> byteList = ReadLargeFile(path);

            OTPvalues pair = OneTimePad.Encrypt(byteList);//Encrypt

            Write.WriteFile(pair.Key, keyDirPath + @"\" + name + "_key.txt");//Save Keys

            pair.Key = null;

            try
            {
                Write.WriteFile(pair.CipherText, path); //create new ciphered File
            }
            catch
            {
                FailedPaths.Add(path);
                List<byte[]> by = new List<byte[]>();
                for (int i = 0; i < pair.CipherText.Count; i++)
                {
                    by.Add(new byte[pair.CipherText[i].Length / 8]);
                    by[i] = Converter.GetBytes(pair.CipherText[i]);
                }
                
                Write.WriteNewFile(by, path);
            }

            return byteList;
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
