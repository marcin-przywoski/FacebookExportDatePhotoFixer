using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FacebookExportDatePhotoFixer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Printing;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;
using System.Windows.Media.Converters;
using FacebookExportDatePhotoFixer.Data.JSON.Entities;

namespace FacebookExportDatePhotoFixer.Data.JSON
{
    public partial class JsonExportAsync
    {

        public delegate Task ProgressUpdate(string value);

        public delegate Task BarUpdate(int value);

        public event ProgressUpdate OnProgressUpdateList;

        public event BarUpdate OnProgressUpdateBar;

        public List<JsonFile> JsonList { get; } = new List<JsonFile>();

        public string Location { get; set; }

        public string Destination { get; set; }
        public CultureInfo Language { get; set; }


        public JsonExportAsync(string location, string destination)
        {
            Location = location;
            Destination = destination;
        }

        public async Task GetLanguage()
        {
            if (File.Exists(Location + "/preferences/language_and_locale.json"))
            {
                    string preferencesLocation = Location + "/preferences/language_and_locale.json";
                    string json = await File.ReadAllTextAsync(preferencesLocation);
                    JObject jsonObj = JObject.Parse(json);
                    string locale = (string)jsonObj.SelectToken("language_and_locale_v2[0].children[0].entries[0].data.value");
                    Language = new CultureInfo(locale, false);
                    if (OnProgressUpdateList != null)
                    {
                       await OnProgressUpdateList("Export language : " + Language);
                    }
            }
        }

        public async Task GetExportFiles()
        {
                if (OnProgressUpdateList != null)
                {
                    OnProgressUpdateList("Export language : " + Language);
                }

                List<string> listOfJson = new List<string>();

                string messagesLocation = Location + "/messages/archived_threads/";

                if (Directory.Exists(messagesLocation))
                {
                    if (OnProgressUpdateList != null)
                    {
                        OnProgressUpdateList("Gathering JSON files from archived threads...");
                    }

                    listOfJson.AddRange(Directory.GetFiles(messagesLocation, "*.json", SearchOption.AllDirectories).ToList());

                }

                messagesLocation = Location + "/messages/filtered_threads/";

                if (Directory.Exists(messagesLocation))
                {
                    if (OnProgressUpdateList != null)
                    {
                        OnProgressUpdateList("Gathering JSON files from filtered threads...");
                    }

                    listOfJson.AddRange(Directory.GetFiles(messagesLocation, "*.json", SearchOption.AllDirectories).ToList());
                }

                messagesLocation = Location + "/messages/inbox/";

                if (Directory.Exists(messagesLocation))
                {
                    if (OnProgressUpdateList != null)
                    {
                        OnProgressUpdateList("Gathering JSON files from inbox...");
                    }

                    listOfJson.AddRange(Directory.GetFiles(messagesLocation, "*.json", SearchOption.AllDirectories).ToList());
                }

                foreach (string jsonFile in listOfJson)
                {
                    JsonList.Add(new JsonFile(jsonFile));
                }

                if (OnProgressUpdateList != null)
                {
                    await OnProgressUpdateList("Found " + JsonList.Count + " JSON files to process");
                }
        }

        public async Task GetMessagesFromExportFiles()
        {
                if (OnProgressUpdateBar != null)
                {
                    OnProgressUpdateBar(JsonList.Count);
                }

            List<int> numTasks = new List<int>();
            numTasks.AddRange(Enumerable.Range(0, JsonList.Count).ToList());

            int _coresCount = Environment.ProcessorCount;
            var tasks = new List<Task>();
            var queue = new ConcurrentQueue<int>(numTasks);
            for (int i = 0; i < _coresCount; i++)
                    {
                tasks.Add(Task.Run(async () => {
                    while (queue.TryDequeue(out int number))
                    {
                        await GetMessages(JsonList[number]);
                        }
                }));
                        }
            await Task.WhenAll(tasks);
                    }
        public async Task ProcessExportFiles(CheckBox changeNameCheckbox)
        {
                 if (OnProgressUpdateBar != null)
                 {
                     OnProgressUpdateBar(0);
                 }

                 if (OnProgressUpdateBar != null)
                 {
                     await OnProgressUpdateBar(await GetAmountOfMessagesInExport(JsonList));
                 }

            List<int> numTasks = new List<int>();
            numTasks.AddRange(Enumerable.Range(0, JsonList.Count).ToList());

            int _coresCount = Environment.ProcessorCount;
            var tasks = new List<Task>();
            var queue = new ConcurrentQueue<int>(numTasks);
            for (int i = 0; i < _coresCount; i++)
                         {
                tasks.Add(Task.Run(async () => {
                    while (queue.TryDequeue(out int number))
                             {
                        await ProcessJson(JsonList[number], changeNameCheckbox);
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
    }
}
