using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.IO;
using FacebookExportDatePhotoFixer.Data.HTML;

namespace FacebookExportDatePhotoFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private BackgroundWorker _listUpdateWorker = new BackgroundWorker();
        string exportLocation;
        string destination;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void SourceLocation_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog chooseFolder = new VistaFolderBrowserDialog();
            bool? isBrowserDialogOpened = chooseFolder.ShowDialog();
            if (isBrowserDialogOpened == true)
            {
                exportLocation = chooseFolder.SelectedPath + "/";
                SourceLocationLabel.Content = SourceLocationLabel.Content + exportLocation;
                SourceLocationLabel.ToolTip = SourceLocationLabel.Content;
            }
        }

        private void DestinationLocation_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog chooseFolder = new VistaFolderBrowserDialog();
            bool? isBrowserDialogOpened = chooseFolder.ShowDialog();
            if (isBrowserDialogOpened == true)
            {
                destination = chooseFolder.SelectedPath + "/";
                DestinationLocationLabel.Content = DestinationLocationLabel.Content + chooseFolder.SelectedPath + "/";
                DestinationLocationLabel.ToolTip = DestinationLocationLabel.Content;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(destination) || String.IsNullOrEmpty(exportLocation))
            {
                MessageBox.Show("One of the paths have not been selected!");
            }
            else
            {
                if(CheckExportType(exportLocation) == "json")
                {
                    //FacebookExport facebookExport = new FacebookExport(exportLocation, destination);
                    //_backgroundWorker.DoWork += (obj, e) => WorkerDoWork(facebookExport);
                    //_backgroundWorker.RunWorkerAsync();
                }
                else if (CheckExportType(exportLocation) == "html")
                {
                    FacebookExport facebookExport = new FacebookExport(exportLocation,destination);
                    facebookExport.OnProgressUpdateList += Export_OnProgressUpdateList;
                    facebookExport.OnProgressUpdateBar += Export_OnProgressUpdateBar;
                    _backgroundWorker.DoWork += (obj, e) => WorkerDoWork(facebookExport);
                _backgroundWorker.RunWorkerAsync();
                }
                else if (CheckExportType(exportLocation) == "error")
                {
                    MessageBox.Show("Location folder does not contain no HTML nor Json files!");
                }


            }


        }

        private void WorkerDoWork(FacebookExport export)
        {
            export.GetLanguage();
            export.GetHtmlFiles();
            export.GetMessagesFromHtmlFiles();
            export.ProcessHtmlFiles(changeNamesToDates);
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            facebookExport.GetLanguage();
            facebookExport.GetHtmlFiles(Progress, OutputLog);
            facebookExport.GetMessagesFromHtmlFiles(Progress, OutputLog);
            facebookExport.ProcessHtmlFiles(Progress, OutputLog, changeNamesToDates);

        private void Export_OnProgressUpdateList(string text)
        {
            Dispatcher.Invoke(() =>
            {
                OutputLog.Items.Add(text);
                OutputLog.SelectedIndex = OutputLog.Items.Count - 1;
                OutputLog.ScrollIntoView(OutputLog.SelectedItem);
            });
        }

        private void Export_OnProgressUpdateBar(int value)
        {
            if (value > 1)
            {
                Dispatcher.Invoke(() =>
                {
                    Progress.Maximum = value;
                });
            }
            else if(value == 1)
            {
                Dispatcher.Invoke(() =>
                {
                    Progress.Value++;
                });
            }
            else if (value == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    Progress.Value = 0;
                });
            }

        }

        private string CheckExportType(string Location)
        {
            string[] json = Directory.GetFiles(Location, "*.json",SearchOption.AllDirectories);
            string[] html = Directory.GetFiles(Location, "*.html", SearchOption.AllDirectories);
            if (json.Length != 0)
            {
                return "json";
            }
            else if (html.Length != 0)
            {
                return "html";
            }
            else
            {
                return "error";
            }
        }
    }
}
