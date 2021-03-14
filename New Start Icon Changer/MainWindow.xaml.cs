using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace New_Start_Icon_Changer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string destinationPath = "", exeName = "", imagePath = "";
        public bool imageSelected = false, programSelected = false, showTextOnIcon = false;
        public MainWindow()
        {
            InitializeComponent();
            AddRegistryKey();
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                FileName = "Portable Network Graphics",
                DefaultExt = ".png",
                Filter = "Image files (*.png) | *.png"
            };
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                imageSelected = true;
                string nameOfImage = dialog.FileName[(dialog.FileName.LastIndexOf('\\') + 1)..];
                ImageStatus.Text = nameOfImage + " selected.";
                ImageStatus.Foreground = new SolidColorBrush(Colors.Green);
                imagePath = dialog.FileName;
            }
            ReadyButtonEnable();
        }

        private void ChooseProgram_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                FileName = "Executable Files",
                DefaultExt = ".exe",
                Filter = "Executable Files (*.exe) | *.exe"
            };
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                string fileName = dialog.FileName;
                programSelected = true;
                string nameOfExe = fileName[(fileName.LastIndexOf('\\') + 1)..];
                ProgramStatus.Text = nameOfExe + " selected.";
                ProgramStatus.Foreground = new SolidColorBrush(Colors.Green);
                int lastSlash = fileName.LastIndexOf('\\');
                destinationPath = "";
                for (int i = 0; i < lastSlash; i++)
                {
                    destinationPath += fileName[i];
                }
                exeName = "";
                for (int i = 0; i < nameOfExe.LastIndexOf('.'); i++)
                {
                    exeName += nameOfExe[i];
                }
            }
            ReadyButtonEnable();
        }
        private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to show the program name on the icon?", "Show program name?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            showTextOnIcon = result == MessageBoxResult.Yes;
            CreateXMLFile();
            if (File.Exists(destinationPath + "\\tile.png"))
            {
                File.Delete(destinationPath + "\\tile.png");
            }
            Debug.WriteLine("PLACEHOLDER");
            Debug.WriteLine(imagePath);
            Debug.WriteLine(destinationPath + "\\tile.png");
            File.Move(imagePath, destinationPath + "\\tile.png");
            RestartExplorer();
        }

        public void CreateXMLFile()
        {
            XmlDocument document = new XmlDocument();
            XmlNode applicationNode = document.CreateElement("Application");
            XmlAttribute xmlns = document.CreateAttribute("xmlns:xsi");
            xmlns.Value = "http://www.w3.org/2001/XMLSchema-instance";
            applicationNode.Attributes.Append(xmlns);
            document.AppendChild(applicationNode);

            XmlNode viselsNode = document.CreateElement("VisualElements");
            XmlAttribute[] atts = new XmlAttribute[5];
            string[] keys = { "BackgroundColor", "ShowNameOnSquare150x150Logo", "Square150x150Logo", "Square70x70Logo", "ForegroundText" };
            string[] vals = { "#9146FF", showTextOnIcon ? "on" : "off", "tile.png", "tile.png", "dark" };
            for (int i = 0; i < 5; i++)
            {
                atts[i] = document.CreateAttribute(keys[i]);
                atts[i].Value = vals[i];
                viselsNode.Attributes.Append(atts[i]);
            }
            applicationNode.AppendChild(viselsNode);
            Debug.WriteLine("XML File Saved to " + destinationPath + "\\test.xml");
            using FileStream fs = new FileStream(destinationPath + "\\" + exeName + ".VisualElementsManifest.xml", FileMode.Create);
            document.Save(fs);

        }

        public void AddRegistryKey()
        {
            var hklm = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            RegistryKey key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell\StateStore\ResetCache");
            if (key == null)
            {
                RegistryKey newKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell\StateStore");
                newKey.SetValue("ResetCache", "1", RegistryValueKind.DWord);
                newKey.Close();
            }
        }

        public void ReadyButtonEnable()
        {
            if (imageSelected && programSelected)
            {
                ReadyButton.IsEnabled = true;
                ReadyButton.Content = "Change Icon";
            }
        }

        public void RestartExplorer()
        {
            MessageBoxResult result = MessageBox.Show("The Windows Explorer process needs to be restarted in order to continue. Restart it now? (This does not restart your computer and usually takes around 10 seconds to complete).", "Explorer Needs to be Restarted", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            switch (result)
            {
                case MessageBoxResult.No:
                    MessageBox.Show("Please restart explorer at some point for this to work.", "Restart Explorer");
                    return;
            }
            foreach (Process exe in Process.GetProcesses())
            {
                if (exe.ProcessName == "explorer")
                    exe.Kill();
            }
            Process.Start("explorer.exe");
            MessageBox.Show("Windows Explorer Restarted Successfully", "Restart Successful");
        }
    }
}
