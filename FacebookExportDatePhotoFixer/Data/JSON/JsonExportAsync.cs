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
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FacebookExportDatePhotoFixer.Data.JSON
{
    public partial class JsonExportAsync
    {
        public Subject<string> outputLogSubject = new Subject<string>();

        public IObservable<string> OutputLog => outputLogSubject.AsObservable();

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

                outputLogSubject.OnNext("Export language : " + Language + "\n");
            }
        }

        public async Task GetExportFiles()
        {
            List<string> listOfJson = new List<string>();

                string messagesLocation = Location + "/messages/archived_threads/";

                if (Directory.Exists(messagesLocation))
                {

                outputLogSubject.OnNext("Gathering JSON files from archived threads..." + "\n");

                listOfJson.AddRange(Directory.GetFiles(messagesLocation, "*.json", SearchOption.AllDirectories).ToList());

                }

                messagesLocation = Location + "/messages/filtered_threads/";

                if (Directory.Exists(messagesLocation))
                {

                outputLogSubject.OnNext("Gathering JSON files from filtered threads..." + "\n");

                listOfJson.AddRange(Directory.GetFiles(messagesLocation, "*.json", SearchOption.AllDirectories).ToList());
                }

                messagesLocation = Location + "/messages/inbox/";

                if (Directory.Exists(messagesLocation))
                {

                outputLogSubject.OnNext("Gathering JSON files from filtered threads..." + "\n");

                listOfJson.AddRange(Directory.GetFiles(messagesLocation, "*.json", SearchOption.AllDirectories).ToList());
                }

                foreach (string jsonFile in listOfJson)
                {
                    JsonList.Add(new JsonFile(jsonFile));
                }

            outputLogSubject.OnNext("Found " + JsonList.Count + " JSON files to process" + "\n");
        }

        public async Task GetMessagesFromExportFiles()
        {
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

            this.JsonList.RemoveAll(s=> s.Conversation.Messages.Count == 0);
        }
        public async Task ProcessExportFiles(CheckBox changeNameCheckbox)
        {
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

            outputLogSubject.OnNext("Done!" + "\n");

            outputLogSubject.OnCompleted();
        }
    }
}
