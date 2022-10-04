using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FacebookExportDatePhotoFixer.Interfaces;
using HtmlAgilityPack;

namespace FacebookExportDatePhotoFixer.Data.HTML
{
    partial class HtmlExport
    {
        public delegate Task ProgressUpdate(string value);

        public event ProgressUpdate OnProgressUpdateList;

        public List<HtmlFile> HtmlList { get; } = new List<HtmlFile>();

        public string Location { get; set; }

        public string Destination { get; set; }

        public CultureInfo Language { get; set; }

        public HtmlExport(string location, string destination)
        {
            Location = location;
            Destination = destination;
        }

        public async Task GetLanguage()
        {
            await Task.Run(async () =>
            {
                if (File.Exists(Location + "/about_you/preferences.html"))
                {
                    string preferencesLocation = Location + "/about_you/preferences.html";
                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.Load(preferencesLocation);
                    string locale = htmlDocument.DocumentNode.SelectSingleNode("/ html / body / div / div / div / div[2] / div[2] / div / div[3] / div / div[2] / div[1] / div[2] / div / div / div / div[1] / div[3]").InnerText;
                    Language = new CultureInfo(locale, false);
                }
                else if (File.Exists(Location + "/preferences/language_and_locale.html"))
                {
                    string preferencesLocation = Location + "/preferences/language_and_locale.html";
                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.Load(preferencesLocation);
                    string locale = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div/div/div/div[2]/div[2]/div/div[1]/div/div[2]/div[1]/div[2]/div/div/div/div[1]/div[3]").InnerText;
                    Language = new CultureInfo(locale, false);
                }
            });
        }

        public async Task GetExportFiles()
        {
                if (OnProgressUpdateList != null)
                {
                    OnProgressUpdateList("Export language : " + Language);
                }

                List<string> listOfHtml = new List<string>();
                string messagesLocation = Location + "/messages/archived_threads/";

                if (OnProgressUpdateList != null)
                {
                    OnProgressUpdateList("Gathering HTML files from archived threads...");
                }
                listOfHtml = Directory.GetFiles(messagesLocation, "*.html", SearchOption.AllDirectories).ToList();

                if (OnProgressUpdateList != null)
                {
                    OnProgressUpdateList("Gathering HTML files from filtered threads...");
                }
                messagesLocation = Location + "/messages/filtered_threads/";
                listOfHtml.AddRange(Directory.GetFiles(messagesLocation, "*.html", SearchOption.AllDirectories).ToList());

                if (OnProgressUpdateList != null)
                {
                    OnProgressUpdateList("Gathering HTML files from inbox...");
                }
                messagesLocation = Location + "/messages/inbox/";
                listOfHtml.AddRange(Directory.GetFiles(messagesLocation, "*.html", SearchOption.AllDirectories).ToList());

                foreach (string htmlFile in listOfHtml)
                {
                    HtmlList.Add(new HtmlFile(htmlFile));
                }

                if (OnProgressUpdateList != null)
                {
                    OnProgressUpdateList("Found " + HtmlList.Count + " HTML files to process");
                }
        }

        public async Task GetMessagesFromExportFiles()
        {
            List<int> numTasks = new List<int>();
            numTasks.AddRange(Enumerable.Range(0, HtmlList.Count).ToList());

            int _coresCount = Environment.ProcessorCount;
            var tasks = new List<Task>();
            var queue = new ConcurrentQueue<int>(numTasks);
            for (int i = 0; i < _coresCount; i++)
                    {
                tasks.Add(Task.Run(async () =>
                        {
                    while (queue.TryDequeue(out int number))
                            {

                        await GetMessages(HtmlList[number]);
                                        }
                }));
                                    }
            await Task.WhenAll(tasks);

                HtmlList.RemoveAll(s => s.ListOfMessages.Count == 0);
        }

        public async Task ProcessExportFiles(CheckBox changeNameCheckbox)
        {
                        bool? isChecked = changeNameCheckbox.Dispatcher.Invoke(() => changeNameCheckbox.IsChecked);

            List<int> numTasks = new List<int>();
            numTasks.AddRange(Enumerable.Range(0, HtmlList.Count).ToList());

            int _coresCount = Environment.ProcessorCount;
            var tasks = new List<Task>();
            var queue = new ConcurrentQueue<int>(numTasks);
            for (int i = 0; i < _coresCount; i++)
                        {
                tasks.Add(Task.Run(async () =>
                            {
                    while (queue.TryDequeue(out int number))
                                {
                        await ProcessHtml(HtmlList[number], changeNameCheckbox);
                                }
                }));
                            }
            await Task.WhenAll(tasks);

                if (OnProgressUpdateList != null)
                {
                    {
                        OnProgressUpdateList("Done!");
                    }
                }
        }

        private async Task<int> GetAmountOfMessagesInExport(List<HtmlFile> htmlFiles)
        {
            int count = 0;
            await Task.Run(() =>
            {
                foreach (HtmlFile file in htmlFiles)
                {
                    count = +file.MessagesCount;
                }
            });

            return count;
        }
    }
}
