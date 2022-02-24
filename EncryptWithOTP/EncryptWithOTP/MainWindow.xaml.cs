using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EncryptWithOTP
{
    public partial class MainWindow : Window
    {
        List<BitArray> encryptedBits = new List<BitArray>();
        List<BitArray> keyBits = new List<BitArray>();
        int i = 0;

        string path = "";
        string keyPath = "";
        string keyDestinPath = "";
        bool fileHere = false;
        bool keyHere = false;
        int count;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void EncryptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDriveInfo())
            {
                MessageBox.Show("Not enough Space");
                Reset();
                return;
            }

            MessageBoxButton buttons = MessageBoxButton.YesNo;

            var result = MessageBox.Show("Are you sure you want to enycrypt this file(s)?", "encrypt", buttons);
            if(result == MessageBoxResult.Yes)
            {
                var progress = new Progress<int>(value =>
                {
                    ProgressBar.Value = value;
                });

                Read read = new Read(path);

                if (Directory.Exists(path))
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    await Task.Run(() => read.ReadAndEncryptDirectory(path, progress, count, keyDestinPath));
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        ProcessingLbl.Visibility = Visibility.Visible;
                        await Task.Run(() => read.ReadAndEncryptFile(path, fileInfo.Name, fileInfo.Directory.FullName));
                    }
                }
                
                if(read.FailedPaths.Count > 0)
                {
                    string message = "";
                    foreach(string path in read.FailedPaths)
                    {
                        message += path + ";";
                    }
                    message = message.Replace(";", System.Environment.NewLine);
                    MessageBox.Show(read.FailedPaths.Count + " could not be overriden. Please delete these files manually:" + System.Environment.NewLine + message);
                }

                Reset();
            }
        }

        private async void DecryptBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            var result = MessageBox.Show("Are you sure you enterd the right key(s) and file(s)?", "decrypt", buttons);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var progress = new Progress<int>(value =>
                    {
                        ProgressBar.Value = value;
                    });

                    Read read = new Read(path);
                    Read read2 = new Read(keyPath);


                    if (!Directory.Exists(path))
                    {
                        ProcessingLbl.Visibility = Visibility.Visible;
                        await Task.Run(() =>
                        {
                            FileInfo file = new FileInfo(path);
                            var encryptedFile = read.ReadFile(path);
                            var keyFile = read2.ReadFile(keyPath);

                            BitArray fileBits = new BitArray(encryptedFile);
                            BitArray keyFileBits = new BitArray(keyFile);

                            Write.WriteFile(OneTimePad.Decrypt(new OTPvalues(keyFileBits, fileBits)), file.FullName);

                        });

                        ProcessingLbl.Visibility = Visibility.Hidden;

                        MessageBoxButton buttons2 = MessageBoxButton.YesNo;
                        var result2 = MessageBox.Show("Delete Key (recommended)?", "delete", buttons2);
                        if (result2 == MessageBoxResult.Yes)
                        {
                            File.Delete(keyPath);
                        }
                    }
                    else
                    {
                        List<byte[]> encrypted = new List<byte[]>();
                        List<byte[]> key = new List<byte[]>();


                        encrypted = read.ReadDirectory(path);
                        key = read2.ReadDirectory(keyPath);

                        for (int i = 0; i < encrypted.Count; i++)
                        {
                            encryptedBits.Add(new BitArray(encrypted[i]));
                            keyBits.Add(new BitArray(key[i]));
                        }

                        ProgressBar.Visibility = Visibility.Visible;
                        await Task.Run(() => DecryptAndWrite(path, progress));
                        ProgressBar.Visibility = Visibility.Hidden;

                        MessageBoxButton buttons2 = MessageBoxButton.YesNo;
                        var result2 = MessageBox.Show("Delete Keys (recommended)?","delete", buttons2);
                        if(result2 == MessageBoxResult.Yes)
                        {
                            DeleteKeys(keyPath);
                        }

                        encrypted.Clear();
                        key.Clear();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Probably wrong Folder or Wrong key!");
                    MessageBox.Show(ex.ToString());
                }

                encryptedBits.Clear();
                keyBits.Clear();
                Reset();
            }
        }

        public void DecryptAndWrite(string path, IProgress<int> progress)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.GetFiles())
            {
                //file.Delete();
                Write.WriteFile(OneTimePad.Decrypt(new OTPvalues(keyBits[i], encryptedBits[i])), file.FullName);
                i++;
                var percentageComplete = (i * 100) / (count / 4);
                progress.Report(percentageComplete);
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                DecryptAndWrite(dir.FullName, progress);
            }
        }

        public void DeleteKeys(string keyPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(keyPath);

            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                DeleteKeys(dir.FullName);
            }

            dirInfo.Delete();
        }

        private void FileLbl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                path = files[0];

                FilePanel.Visibility = Visibility.Visible;
                FilePathLbl.Content = path;
                if (Directory.Exists(path))
                {
                    count = GetCount(path);
                }
            }

            EncryptBtn.IsEnabled = true;
            EncryptBtn.Foreground = (Brush?)(new BrushConverter().ConvertFrom("#F5F2E7"));
            fileHere = true;

            if (keyHere)
            {
                DecryptBtn.IsEnabled = true;
                DecryptBtn.Foreground = (Brush?)(new BrushConverter().ConvertFrom("#F5F2E7"));

                EncryptBtn.IsEnabled = false;
                EncryptBtn.Foreground = Brushes.Black;
            }
        }

        private void KeyLbl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                keyPath = files[0];
                KeyPanel.Visibility = Visibility.Visible;
                KeyPathLbl.Content = keyPath;
                if (Directory.Exists(keyPath))
                {
                    count = GetCount(keyPath);
                }
            }

            if (fileHere)
            {
                DecryptBtn.IsEnabled = true;
                DecryptBtn.Foreground = (Brush?)(new BrushConverter().ConvertFrom("#F5F2E7"));

                EncryptBtn.IsEnabled = false;
                EncryptBtn.Foreground = Brushes.Black;
            }

            keyHere = true;
        }

        bool CheckDriveInfo()
        {
            long freeSpace = 0;
            DriveInfo drive = new DriveInfo(path[0].ToString());
            if (drive.IsReady)
            {
                freeSpace = drive.AvailableFreeSpace;
            }

            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                if (GetDirectorySize(path) * 2 + 1000000 < freeSpace)
                {
                    return true;
                }
                return false;
            }
            else
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Length * 2 + 1000000 < freeSpace)
                {
                    return true;
                }
                return false;
            }
        }

        static long GetDirectorySize(string p)
        {
            //Definitly not stolen 

            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(p, "*.*", SearchOption.AllDirectories);

            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4.
            // Return total size
            return b;
        }

        private void RemoveFileElli_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EncryptBtn.IsEnabled = false;
            EncryptBtn.Foreground = Brushes.Black;
            fileHere = false;
            path = "";
            FilePathLbl.Content = "";
            FilePanel.Visibility = Visibility.Hidden;
            DecryptBtn.IsEnabled = false;
            DecryptBtn.Foreground = Brushes.Black;
        }

        private void RemoveKeyElli_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DecryptBtn.IsEnabled = false;
            DecryptBtn.Foreground = Brushes.Black;
            keyHere = false;
            keyPath = "";
            KeyPanel.Visibility = Visibility.Hidden;
            KeyPathLbl.Content = "";
        }

        int GetCount(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            count += dirInfo.GetFiles().Length;
            foreach (var dir in dirInfo.GetDirectories())
            {
                GetCount(dir.FullName.ToString());
            }
            return count;
        }

        void Reset()
        {
            EncryptBtn.IsEnabled = false;
            EncryptBtn.Foreground = Brushes.Black;
            DecryptBtn.IsEnabled = false;
            DecryptBtn.Foreground = Brushes.Black;
            fileHere = false;
            keyHere = false;
            path = "";
            keyPath = "";
            i = 0;
            KeyPanel.Visibility = Visibility.Hidden;
            FilePanel.Visibility = Visibility.Hidden;
            FilePathLbl.Content = "";
            KeyPathLbl.Content = "";
            ProgressBar.Visibility = Visibility.Hidden;
            ProgressBar.Value = 0;
            count = 0;
            ProcessingLbl.Visibility = Visibility.Hidden;
            keyDestinPath = "";
        }

        private void FileBtn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog();
            ofd.IsFolderPicker = true;
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = ofd.FileName;

                FilePanel.Visibility = Visibility.Visible;
                FilePathLbl.Content = path;
                if (Directory.Exists(path))
                {
                    count = GetCount(path);
                }

                EncryptBtn.IsEnabled = true;
                EncryptBtn.Foreground = (Brush?)(new BrushConverter().ConvertFrom("#F5F2E7"));
                fileHere = true;

                if (keyHere)
                {
                    DecryptBtn.IsEnabled = true;
                    DecryptBtn.Foreground = (Brush?)(new BrushConverter().ConvertFrom("#F5F2E7"));

                    EncryptBtn.IsEnabled = false;
                    EncryptBtn.Foreground = Brushes.Black;
                }
            }
        }

        private void KeyBtn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog();
            ofd.IsFolderPicker = true;
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                keyPath = ofd.FileName;

                KeyPanel.Visibility = Visibility.Visible;
                KeyPathLbl.Content = keyPath;
                if (Directory.Exists(keyPath))
                {
                    count = GetCount(keyPath);
                }

                if (fileHere)
                {
                    DecryptBtn.IsEnabled = true;
                    DecryptBtn.Foreground = (Brush?)(new BrushConverter().ConvertFrom("#F5F2E7"));

                    EncryptBtn.IsEnabled = false;
                    EncryptBtn.Foreground = Brushes.Black;
                }

                keyHere = true;
            }
        }

        private void KeyLocationBtn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog();
            ofd.IsFolderPicker = true;
            if(ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                keyDestinPath = ofd.FileName;
            }
        }
    }
}
