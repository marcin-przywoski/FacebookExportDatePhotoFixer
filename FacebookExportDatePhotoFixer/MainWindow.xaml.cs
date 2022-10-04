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
using FacebookExportDatePhotoFixer.Data.JSON;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Threading;

namespace FacebookExportDatePhotoFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();
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
                SourceLocationLabel.Content = "Source location : ";
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
                DestinationLocationLabel.Content = "Destination location : ";
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
                if (CheckExportType(exportLocation) == "json")
                {
                    JsonExportAsync jsonExportAsync = new JsonExportAsync(exportLocation, destination);
                    IObservable<IList<string>> buffer = jsonExportAsync.OutputLog.Buffer(TimeSpan.FromMilliseconds(1000), 6);
                    IObservable<IList<string>> chunked = buffer.ObserveOnDispatcher(DispatcherPriority.Background);
                    IDisposable update = chunked.Subscribe(name =>
                    {
                        foreach (string item in name)
                        {
                            OutputLog.AppendText(item);
                        }
                        OutputLog.ScrollToEnd();
                    });
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    await jsonExportAsync.GetLanguage();
                    await jsonExportAsync.GetExportFiles();
                    await jsonExportAsync.GetMessagesFromExportFiles();
                    await jsonExportAsync.ProcessExportFiles(changeNamesToDates);
                    stopwatch.Stop();
                    await chunked;
                    await buffer;
                    await Dispatcher.InvokeAsync(() =>
                    {
                        OutputLog.AppendText($"Time elapsed total: {stopwatch.Elapsed:g}. Log saved to {destination} log.txt ");
                        File.WriteAllTextAsync(destination + "log.txt", OutputLog.Text);
                    });
                }
                else if (CheckExportType(exportLocation) == "html")
                {
                    HtmlExport facebookExport = new HtmlExport(exportLocation, destination);
                    IObservable<IList<string>> buffer = facebookExport.OutputLog.Buffer(TimeSpan.FromMilliseconds(1000), 6);
                    IObservable<IList<string>> chunked = buffer.ObserveOnDispatcher(DispatcherPriority.Background);
                    IDisposable update = chunked.Subscribe(name =>
                    {
                        foreach (string item in name)
                        {
                            OutputLog.AppendText(item);
                        }
                        OutputLog.ScrollToEnd();
                    });
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    await facebookExport.GetLanguage();
                    await facebookExport.GetExportFiles();
                    await facebookExport.GetMessagesFromExportFiles();
                    await facebookExport.ProcessExportFiles(changeNamesToDates);
                    stopwatch.Stop();
                    await chunked;
                    await buffer;
                    await Dispatcher.InvokeAsync(() =>
                    {
                        OutputLog.AppendText($"Time elapsed total: {stopwatch.Elapsed:g}. Log saved to {destination} log.txt");
                        File.WriteAllTextAsync(destination + "log.txt", OutputLog.Text);
                    });
                }
                else if (CheckExportType(exportLocation) == "error")
                {
                    MessageBox.Show("Location folder does not contain no HTML nor Json files!");
                }
            }
        }

        private string CheckExportType(string Location)
        {
            string[] json = Directory.GetFiles(Location, "*.json", SearchOption.AllDirectories);
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
