using Windows.UI.Xaml.Controls;
using Microsoft.Win32;
using System;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Start_Icon_Changer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string destinationPath = "", exeName = "";
        public bool imageSelected = false, programSelected = false;
        public StorageFile imageToWrite;
        public MainPage()
        {
            this.InitializeComponent();
            checkPermissions();
        }

        async public void checkPermissions()
        {
            ContentDialog settingsDialog = new ContentDialog()
            {
                Title = "Check File System Permissions",
                Content = "Access to file system for this application is necessary for this program to work. Open settings to ensure compatability?",
                PrimaryButtonText = "Open Settings (Requires Relaunch)",
                CloseButtonText = "I've Enabled this Setting"
            };

            ContentDialogResult result = await settingsDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                CoreApplication.Exit();
            }
            addRegistryKey();
        }

        public void createXMLFile()
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
            string[] vals = { "#9146FF", "on", "tile.png", "tile.png", "dark" };
            for (int i = 0; i < 5; i++)
            {
                atts[i] = document.CreateAttribute(keys[i]);
                atts[i].Value = vals[i];
                viselsNode.Attributes.Append(atts[i]);
            }
            applicationNode.AppendChild(viselsNode);
            using (FileStream fs = new FileStream(destinationPath + "\\test.xml", FileMode.CreateNew))
            {
                document.Save(fs);
            }
        }

        public void addRegistryKey()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell\StateStore\ResetCache");
            if (key == null)
            {
                RegistryKey newKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell\StateStore");
                newKey.SetValue("ResetCache", "1");
                newKey.Close();
            }
        }

        async private void chooseImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".png");
            StorageFile image = await picker.PickSingleFileAsync();
            if (image != null)
            {
                imageSelected = true;
                ImageStatus.Text = image.Name + " selected.";
                ImageStatus.Foreground = new SolidColorBrush(Colors.Green);
                checkIfBothSelected();
            }
            else
            {
                ImageStatus.Text = "Invalid File. Choose another.";
            }
            
        }

        async private void chooseProgram_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".exe");
            StorageFile image = await picker.PickSingleFileAsync();
            if (image != null)
            {
                programSelected = true;
                ProgramStatus.Text = image.Name + " selected.";
                ProgramStatus.Foreground = new SolidColorBrush(Colors.Green);
                string pathToExe = image.Path;
                pathToExe = pathToExe.Substring(0, pathToExe.LastIndexOf('\\'));
                string path = image.Path.ToString();
                exeName = image.Path.Substring(path.LastIndexOf('\\') + 1);
                exeName = exeName.Substring(0, exeName.Length - 4);
                destinationPath = pathToExe;
                checkIfBothSelected();
            }
            else
            {
                ProgramStatus.Text = "Invalid File. Choose another.";
            }
            
        }

        public void checkIfBothSelected()
        {
            if (programSelected && imageSelected)
            {
                ReadyButton.IsEnabled = true;
                ReadyButton.Content = "Change Icon";
            }
        }

        async private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            createXMLFile();
        }
    }
}
