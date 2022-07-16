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

namespace FacebookExportDatePhotoFixer.Data.JSON
{
    class JsonExport : IExport
    {
        public delegate Task ProgressUpdate(string value);

        public delegate Task BarUpdate(int value);

        public event ProgressUpdate OnProgressUpdateList;

        public event BarUpdate OnProgressUpdateBar;

        public List<JsonFile> JsonList { get; } = new List<JsonFile>();

        public string Location { get; set; }

        public string Destination { get; set; }
        public CultureInfo Language { get; set; }


        public JsonExport(string location, string destination)
        {
            Location = location;
            Destination = destination;
        }

        public void GetLanguage()
        {
            if (File.Exists(Location + "/preferences/language_and_locale.json"))
            {
                string preferencesLocation = Location + "/preferences/language_and_locale.json";
                string json = File.ReadAllText(preferencesLocation);
                JObject jsonObj = JObject.Parse(json);
                string locale = (string)jsonObj.SelectToken("language_and_locale_v2[0].children[0].entries[0].data.value");
                Language = new CultureInfo(locale, false);
            }
        }

        public void GetExportFiles()
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
                OnProgressUpdateList("Found " + JsonList.Count + " JSON files to process");
            }

        }

        public void GetMessagesFromExportFiles()
        {
            if (OnProgressUpdateBar != null)
            {
                OnProgressUpdateBar(JsonList.Count);
            }

            foreach (JsonFile file in JsonList)
            {
                if (OnProgressUpdateList != null)
                {
                    OnProgressUpdateList("Processing : " + file.Location);
                }

                if (OnProgressUpdateBar != null)
                {
                    OnProgressUpdateBar(1);
                }

                file.Conversation = JsonConvert.DeserializeObject<Conversation>(File.ReadAllText(file.Location));

                int totalMessagesCount = file.Conversation.Messages.Count;

                file.Conversation.Messages.RemoveAll(s => s.Photos is null && s.Gifs is null && s.Videos is null);

                if (OnProgressUpdateList != null)
                {
                    if (totalMessagesCount != 0 && totalMessagesCount > 1)
                    {
                        OnProgressUpdateList($"There are {totalMessagesCount} messages, of which {file.Conversation.Messages.Count} with linked media");
                    }
                    else if (totalMessagesCount == 1)
                    {
                        OnProgressUpdateList($"There is {totalMessagesCount} message, of which {file.Conversation.Messages.Count} with linked media");
                    }
                }
            }
        }
        public void ProcessExportFiles(CheckBox changeNameCheckbox)
        {
            if (OnProgressUpdateBar != null)
            {
                OnProgressUpdateBar(0);
                OnProgressUpdateBar(GetAmountOfMessagesInExport(JsonList));
            }

            foreach (JsonFile file in JsonList)
            {
                foreach (Conversation.Message message in file.Conversation.Messages)
                {
                    if (OnProgressUpdateBar != null)
                    {
                        OnProgressUpdateBar(1);
                    }

                    bool? isChecked = changeNameCheckbox.Dispatcher.Invoke(() => changeNameCheckbox.IsChecked);

                    if (!(message.Photos is null))
                    {
                        try
                        {
                            if (isChecked == true)
                            {
                                foreach (Conversation.Message.Photo photo in message.Photos)
                                {
                                    if (File.Exists(Location + photo.Uri))
                                    {
                                        if (OnProgressUpdateList != null)
                                        {
                                            OnProgressUpdateList(photo.Uri);
                                        }

                                        Directory.CreateDirectory(Path.GetDirectoryName(Destination + photo.Uri));
                                        string date = message.Date.ToString("yyyyMMdd_HHmmss");
                                        string newName = photo.Uri.Replace(Path.GetFileNameWithoutExtension(photo.Uri), date);
                                        File.Copy(Location + photo.Uri, Destination + newName);
                                        File.SetCreationTime(Destination + newName, message.Date);
                                        File.SetLastAccessTime(Destination + newName, message.Date);
                                        File.SetLastWriteTime(Destination + newName, message.Date);
                                    }
                                }
                            }
                            else
                            {
                                foreach (Conversation.Message.Photo photo in message.Photos)
                                {
                                    if (File.Exists(Location + photo.Uri))

                                        if (OnProgressUpdateList != null)
                                        {
                                            OnProgressUpdateList(photo.Uri);
                                        }
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(Destination + photo.Uri));
                                        File.Copy(Location + photo.Uri, Destination + photo.Uri);
                                        File.SetCreationTime(Destination + photo.Uri, message.Date);
                                        File.SetLastAccessTime(Destination + photo.Uri, message.Date);
                                        File.SetLastWriteTime(Destination + photo.Uri, message.Date);
                                    }
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            if (message.Photos.Exists(s => s.Uri.Contains("stickers_used")))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Photos.Find(url => url.Uri.Contains("stickers_used")).Uri)} is a sticker, skipping");
                                }
                            }
                            else
                            {
                                DateTime dateNewName = message.Date;
                                string date = dateNewName.ToString("yyyyMMdd_HHmmss");
                                string newNameException = message.Photos.First().Uri.Replace(Path.GetFileNameWithoutExtension(message.Photos.First().Uri), date);
                                while (File.Exists(Destination + newNameException))
                                {
                                    if (OnProgressUpdateList != null)
                                    {
                                        OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Photos.First().Uri)} file with same date as name already exists at target location!");
                                        OnProgressUpdateList("Adding 1 second to filename to avoid I/O conflicts");
                                    }
                                    dateNewName = dateNewName.AddSeconds(1);
                                    string dateFixed = dateNewName.ToString("yyyyMMdd_HHmmss");
                                    newNameException = message.Photos.First().Uri.Replace(Path.GetFileNameWithoutExtension(message.Photos.First().Uri), dateFixed);
                                }
                                File.Copy(Location + message.Photos.First().Uri, Destination + newNameException);
                                File.SetCreationTime(Destination + newNameException, message.Date);
                                File.SetLastAccessTime(Destination + newNameException, message.Date);
                                File.SetLastWriteTime(Destination + newNameException, message.Date);
                            }
                        }
                    }
                    if (!(message.Gifs is null))
                    {
                        try
                        {
                            if (isChecked == true)
                            {
                                foreach (Conversation.Message.Gif gif in message.Gifs)
                                {
                                    if (File.Exists(Location + gif.Uri))
                                    {
                                        if (OnProgressUpdateList != null)
                                        {
                                            OnProgressUpdateList(gif.Uri);
                                        }
                                        Directory.CreateDirectory(Path.GetDirectoryName(Destination + gif.Uri));
                                        string date = message.Date.ToString("yyyyMMdd_HHmmss");
                                        string newName = gif.Uri.Replace(Path.GetFileNameWithoutExtension(gif.Uri), date);
                                        File.Copy(Location + gif.Uri, Destination + newName);
                                        File.SetCreationTime(Destination + newName, message.Date);
                                        File.SetLastAccessTime(Destination + newName, message.Date);
                                        File.SetLastWriteTime(Destination + newName, message.Date);
                                    }
                                }
                            }
                            else
                            {
                                foreach (Conversation.Message.Gif gif in message.Gifs)
                                {
                                    if (File.Exists(Location + gif.Uri))
                                    {
                                        if (OnProgressUpdateList != null)
                                        {
                                            OnProgressUpdateList(gif.Uri);
                                        }
                                        Directory.CreateDirectory(Path.GetDirectoryName(Destination + gif.Uri));
                                        File.Copy(Location + gif.Uri, Destination + gif.Uri);
                                        File.SetCreationTime(Destination + gif.Uri, message.Date);
                                        File.SetLastAccessTime(Destination + gif.Uri, message.Date);
                                        File.SetLastWriteTime(Destination + gif.Uri, message.Date);
                                    }
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            if (message.Gifs.Exists(s => s.Uri.Contains("stickers_used")))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Gifs.Find(url => url.Uri.Contains("stickers_used")).Uri)} is a sticker, skipping");
                                }
                            }
                            else
                            {
                                DateTime dateNewName = message.Date;
                                string date = dateNewName.ToString("yyyyMMdd_HHmmss");
                                string newNameException = message.Gifs.First().Uri.Replace(Path.GetFileNameWithoutExtension(message.Gifs.First().Uri), date);
                                while (File.Exists(Destination + newNameException))
                                {
                                    if (OnProgressUpdateList != null)
                                    {
                                        OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Gifs.First().Uri)} file with same date as name already exists at target location!");
                                        OnProgressUpdateList("Adding 1 second to filename to avoid I/O conflicts");
                                    }
                                    dateNewName = dateNewName.AddSeconds(1);
                                    string dateFixed = dateNewName.ToString("yyyyMMdd_HHmmss");
                                    newNameException = message.Gifs.First().Uri.Replace(Path.GetFileNameWithoutExtension(message.Gifs.First().Uri), dateFixed);
                                }
                                File.Copy(Location + message.Gifs.First().Uri, Destination + newNameException);
                                File.SetCreationTime(Destination + newNameException, message.Date);
                                File.SetLastAccessTime(Destination + newNameException, message.Date);
                                File.SetLastWriteTime(Destination + newNameException, message.Date);
                            }
                        }
                    }
                    if (!(message.Videos is null))
                    {
                        try
                        {
                            if (isChecked == true)
                            {
                                foreach (Conversation.Message.Video video in message.Videos)
                                {
                                    if (File.Exists(Location + video.Uri))
                                    {
                                        if (OnProgressUpdateList != null)
                                        {
                                            OnProgressUpdateList(video.Uri);
                                        }
                                        Directory.CreateDirectory(Path.GetDirectoryName(Destination + video.Uri));
                                        string date = message.Date.ToString("yyyyMMdd_HHmmss");
                                        string newName = video.Uri.Replace(Path.GetFileNameWithoutExtension(video.Uri), date);
                                        File.Copy(Location + video.Uri, Destination + newName);
                                        File.SetCreationTime(Destination + newName, message.Date);
                                        File.SetLastAccessTime(Destination + newName, message.Date);
                                        File.SetLastWriteTime(Destination + newName, message.Date);
                                    }
                                }
                            }
                            else
                            {
                                foreach (Conversation.Message.Video video in message.Videos)
                                {
                                    if (File.Exists(Location + video.Uri))
                                    {
                                        if (OnProgressUpdateList != null)
                                        {
                                            OnProgressUpdateList(video.Uri);
                                        }
                                        Directory.CreateDirectory(Path.GetDirectoryName(Destination + video.Uri));
                                        File.Copy(Location + video.Uri, Destination + video.Uri);
                                        File.SetCreationTime(Destination + video.Uri, message.Date);
                                        File.SetLastAccessTime(Destination + video.Uri, message.Date);
                                        File.SetLastWriteTime(Destination + video.Uri, message.Date);
                                    }
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            if (message.Videos.Exists(s => s.Uri.Contains("stickers_used")))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Videos.Find(url => url.Uri.Contains("stickers_used")).Uri)} is a sticker, skipping");
                                }
                            }
                            else
                            {
                                DateTime dateNewName = message.Date;
                                string date = dateNewName.ToString("yyyyMMdd_HHmmss");
                                string newNameException = message.Videos.First().Uri.Replace(Path.GetFileNameWithoutExtension(message.Videos.First().Uri), date);
                                while (File.Exists(Destination + newNameException))
                                {
                                    if (OnProgressUpdateList != null)
                                    {
                                        OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Videos.First().Uri)} file with same date as name already exists at target location!");
                                        OnProgressUpdateList("Adding 1 second to filename to avoid I/O conflicts");
                                    }
                                    dateNewName = dateNewName.AddSeconds(1);
                                    string dateFixed = dateNewName.ToString("yyyyMMdd_HHmmss");
                                    newNameException = message.Videos.First().Uri.Replace(Path.GetFileNameWithoutExtension(message.Videos.First().Uri), dateFixed);
                                }
                                File.Copy(Location + message.Videos.First().Uri, Destination + newNameException);
                                File.SetCreationTime(Destination + newNameException, message.Date);
                                File.SetLastAccessTime(Destination + newNameException, message.Date);
                                File.SetLastWriteTime(Destination + newNameException, message.Date);
                            }
                        }
                    }


                }
            }
            if (OnProgressUpdateList != null)
            {
                {
                    OnProgressUpdateList("Done!");
                }
            }

        }
        public int GetAmountOfMessagesInExport(List<JsonFile> jsonFiles)
        {
            int count = 0;

            foreach (JsonFile file in jsonFiles)
            {
                count = (int)+file.TotalMessagesCount;
            }
            return (int)count;
        }
    }
}
