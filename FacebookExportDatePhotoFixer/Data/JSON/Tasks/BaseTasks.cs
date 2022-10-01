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
using System.Threading.Tasks.Dataflow;

namespace FacebookExportDatePhotoFixer.Data.JSON
{
    public partial class JsonExportAsync
    {


       private async Task GetMessages(JsonFile json)
        {
            if (OnProgressUpdateList != null)
            {
               await OnProgressUpdateList("Processing : " + json.Location);
            }

            json.Conversation = JsonConvert.DeserializeObject<Conversation>(await File.ReadAllTextAsync(json.Location));

            int totalMessagesCount = json.Conversation.Messages.Count;

            json.Conversation.Messages.RemoveAll(s => s.Photos is null && s.Gifs is null && s.Videos is null);

            if (OnProgressUpdateList != null)
            {
                if (totalMessagesCount != 0 && totalMessagesCount > 1)
                {
                    await OnProgressUpdateList($"There are {totalMessagesCount} messages, of which {json.Conversation.Messages.Count} with linked media");
                }
                else if (totalMessagesCount == 1)
                {
                    await OnProgressUpdateList($"There is {totalMessagesCount} message, of which {json.Conversation.Messages.Count} with linked media");
                }
            }
        }

        private async Task ProcessJson(JsonFile json, CheckBox changeNameCheckbox)
        {
            bool? isChecked = changeNameCheckbox.Dispatcher.Invoke(() => changeNameCheckbox.IsChecked);

            //List<Task> listOfTasks = new List<Task>().AddRange(ProcessMessage(Enumerable.Range(0, json.Conversation.Messages.Count)));

            List<int> numTasks = new List<int>();
            numTasks.AddRange(Enumerable.Range(0, json.Conversation.Messages.Count).ToList());

            int _coresCount = Environment.ProcessorCount;
            var tasks = new List<Task>();
            var queue = new ConcurrentQueue<int>(numTasks);
            for (int i = 0; i < _coresCount; i++)
            {
                tasks.Add(Task.Run(async () => {
                    while (queue.TryDequeue(out int number))
                    {
                        await ProcessMessage(json.Conversation.Messages[number], isChecked);
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }
        internal async Task ProcessMessage(Message message, bool? isChecked)
        {
            int _coresCount = Environment.ProcessorCount;

            switch (message)
            {
                case Message item when (item.Photos != null):
                    List<int> photosTasks = new List<int>();
                    List<Task> _photosTasks = new List<Task>();
                    photosTasks.AddRange(Enumerable.Range(0, message.Photos.Count).ToList());
                    ConcurrentQueue<int> _photosQueue = new ConcurrentQueue<int>(photosTasks);

                    for (int i = 0; i < _coresCount; i++)
                    {
                        _photosTasks.Add(Task.Run(async () => {
                            while (_photosQueue.TryDequeue(out int number))
                            {
                                await ProcessPhoto(message.Photos[number], message.Date, isChecked);
                            }
                        }));
                    }
                    await Task.WhenAll(_photosTasks);

                    break;

                case Message item when (item.Gifs != null):
                    List<int> gifTasks = new List<int>();
                    List<Task> _gifTasks = new List<Task>();
                    gifTasks.AddRange(Enumerable.Range(0, message.Gifs.Count).ToList());

                    ConcurrentQueue<int> _gifsQueue = new ConcurrentQueue<int>(gifTasks);
                    for (int i = 0; i < _coresCount; i++)
                    {
                        _gifTasks.Add(Task.Run(async () => {
                            while (_gifsQueue.TryDequeue(out int number))
                            {
                                await ProcessGif(message.Gifs[number], message.Date, isChecked);
                            }
                        }));
                    }
                    await Task.WhenAll(_gifTasks);

                    break;
                case Message item when (item.Videos != null):
                    List<int> videoTasks = new List<int>();
                    List<Task> _videoTasks = new List<Task>();
                    videoTasks.AddRange(Enumerable.Range(0, message.Videos.Count).ToList());

                    ConcurrentQueue<int> queue = new ConcurrentQueue<int>(videoTasks);
                    for (int i = 0; i < _coresCount; i++)
                    {
                        _videoTasks.Add(Task.Run(async () => {
                            while (queue.TryDequeue(out int number))
                            {
                                await ProcessVideo(message.Videos[number], message.Date, isChecked);
                            }
                        }));
                    }
                    await Task.WhenAll(_videoTasks);
                    break;
            }
        }

        internal async Task ProcessPhoto(Photo photo,DateTime date, bool? isChecked)
        {
            switch (isChecked)
            {
                case true:
                    try
                    {
                            if (File.Exists(Location + photo.Uri))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList(photo.Uri);
                                }
                                Directory.CreateDirectory(Path.GetDirectoryName(Destination + photo.Uri));
                                string fileDate = date.ToString("yyyyMMdd_HHmmss");
                                string newName = photo.Uri.Replace(Path.GetFileNameWithoutExtension(photo.Uri), fileDate);
                                File.Copy(Location + photo.Uri, Destination + newName);
                                File.SetCreationTime(Destination + newName, date);
                                File.SetLastAccessTime(Destination + newName, date);
                                File.SetLastWriteTime(Destination + newName, date);
                            }
                    }
                    catch (IOException)
                    {

                        if (photo.Uri.Contains("stickers_used"))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(photo.Uri)} is a sticker, skipping");
                            }
                        }
                        else
                        {
                            DateTime dateNewName = date;
                            string dateToString = dateNewName.ToString("yyyyMMdd_HHmmss");
                            string newNameException = photo.Uri.Replace(Path.GetFileNameWithoutExtension(photo.Uri), dateToString);
                            while (File.Exists(Destination + newNameException))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(photo.Uri)} file with same date as name already exists at target location!");
                                    OnProgressUpdateList("Adding 1 second to filename to avoid I/O conflicts");
                                    //change adding seconds to appending "_1" to filename
                                }
                                dateNewName = dateNewName.AddSeconds(1);
                                string dateFixed = dateNewName.ToString("yyyyMMdd_HHmmss");
                                newNameException = photo.Uri.Replace(Path.GetFileNameWithoutExtension(photo.Uri), dateFixed);
                            }
                            File.Copy(Location + photo.Uri, Destination + newNameException);
                            File.SetCreationTime(Destination + newNameException, date);
                            File.SetLastAccessTime(Destination + newNameException, date);
                            File.SetLastWriteTime(Destination + newNameException, date);
                        }
                    }

                    break;

                case false:
                    try
                    {
                        if (File.Exists(Location + photo.Uri))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList(photo.Uri);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(Destination + photo.Uri));
                            File.Copy(Location + photo.Uri, Destination + photo.Uri);
                            File.SetCreationTime(Destination + photo.Uri, date);
                            File.SetLastAccessTime(Destination + photo.Uri, date);
                            File.SetLastWriteTime(Destination + photo.Uri, date);
                        }
                    }
                    catch (IOException)
                    {
                        if (photo.Uri.Contains("stickers_used"))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(photo.Uri)} is a sticker, skipping");
                            }
                        }
                    }
                    break;
            }
        }

        internal async Task ProcessGif(Gif gif, DateTime date, bool? isChecked)
        {
            switch (isChecked)
            {
                case true:
                    try
                    {
                        if (File.Exists(Location + gif.Uri))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList(gif.Uri);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(Destination + gif.Uri));
                            string fileDate = date.ToString("yyyyMMdd_HHmmss");
                            string newName = gif.Uri.Replace(Path.GetFileNameWithoutExtension(gif.Uri), fileDate);
                            File.Copy(Location + gif.Uri, Destination + newName);
                            File.SetCreationTime(Destination + newName, date);
                            File.SetLastAccessTime(Destination + newName, date);
                            File.SetLastWriteTime(Destination + newName, date);
                        }
                    }
                    catch (IOException)
                    {

                        if (gif.Uri.Contains("stickers_used"))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(gif.Uri)} is a sticker, skipping");
                            }
                        }
                        else
                        {
                            DateTime dateNewName = date;
                            string dateToString = dateNewName.ToString("yyyyMMdd_HHmmss");
                            string newNameException = gif.Uri.Replace(Path.GetFileNameWithoutExtension(gif.Uri), dateToString);
                            while (File.Exists(Destination + newNameException))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(gif.Uri)} file with same date as name already exists at target location!");
                                    OnProgressUpdateList("Adding 1 second to filename to avoid I/O conflicts");
                                    //change adding seconds to appending "_1" to filename
                                }
                                dateNewName = dateNewName.AddSeconds(1);
                                string dateFixed = dateNewName.ToString("yyyyMMdd_HHmmss");
                                newNameException = gif.Uri.Replace(Path.GetFileNameWithoutExtension(gif.Uri), dateFixed);
                            }
                            File.Copy(Location + gif.Uri, Destination + newNameException);
                            File.SetCreationTime(Destination + newNameException, date);
                            File.SetLastAccessTime(Destination + newNameException, date);
                            File.SetLastWriteTime(Destination + newNameException, date);
                        }
                    }

                    break;

                case false:
                    try
                    {
                        if (File.Exists(Location + gif.Uri))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList(gif.Uri);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(Destination + gif.Uri));
                            File.Copy(Location + gif.Uri, Destination + gif.Uri);
                            File.SetCreationTime(Destination + gif.Uri, date);
                            File.SetLastAccessTime(Destination + gif.Uri, date);
                            File.SetLastWriteTime(Destination + gif.Uri, date);
                        }
                    }
                    catch (IOException)
                    {
                        if (gif.Uri.Contains("stickers_used"))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(gif.Uri)} is a sticker, skipping");
                            }
                        }
                    }
                    break;
            }
        }

        internal async Task ProcessVideo(Video video, DateTime date, bool? isChecked)
        {
            switch (isChecked)
            {
                case true:
                    try
                    {
                        if (File.Exists(Location + video.Uri))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList(video.Uri);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(Destination + video.Uri));
                            string fileDate = date.ToString("yyyyMMdd_HHmmss");
                            string newName = video.Uri.Replace(Path.GetFileNameWithoutExtension(video.Uri), fileDate);
                            File.Copy(Location + video.Uri, Destination + newName);
                            File.SetCreationTime(Destination + newName, date);
                            File.SetLastAccessTime(Destination + newName, date);
                            File.SetLastWriteTime(Destination + newName, date);
                        }
                    }
                    catch (IOException)
                    {

                        if (video.Uri.Contains("stickers_used"))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(video.Uri)} is a sticker, skipping");
                            }
                        }
                        else
                        {
                            DateTime dateNewName = date;
                            string dateToString = dateNewName.ToString("yyyyMMdd_HHmmss");
                            string newNameException = video.Uri.Replace(Path.GetFileNameWithoutExtension(video.Uri), dateToString);
                            while (File.Exists(Destination + newNameException))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(video.Uri)} file with same date as name already exists at target location!");
                                    OnProgressUpdateList("Adding 1 second to filename to avoid I/O conflicts");
                                    //change adding seconds to appending "_1" to filename
                                }
                                dateNewName = dateNewName.AddSeconds(1);
                                string dateFixed = dateNewName.ToString("yyyyMMdd_HHmmss");
                                newNameException = video.Uri.Replace(Path.GetFileNameWithoutExtension(video.Uri), dateFixed);
                            }
                            File.Copy(Location + video.Uri, Destination + newNameException);
                            File.SetCreationTime(Destination + newNameException, date);
                            File.SetLastAccessTime(Destination + newNameException, date);
                            File.SetLastWriteTime(Destination + newNameException, date);
                        }
                    }

                    break;

                case false:
                    try
                    {
                        if (File.Exists(Location + video.Uri))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList(video.Uri);
                            }
                            Directory.CreateDirectory(Path.GetDirectoryName(Destination + video.Uri));
                            File.Copy(Location + video.Uri, Destination + video.Uri);
                            File.SetCreationTime(Destination + video.Uri, date);
                            File.SetLastAccessTime(Destination + video.Uri, date);
                            File.SetLastWriteTime(Destination + video.Uri, date);
                        }
                    }
                    catch (IOException)
                    {
                        if (video.Uri.Contains("stickers_used"))
                        {
                            if (OnProgressUpdateList != null)
                            {
                                OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(video.Uri)} is a sticker, skipping");
                            }
                        }
                    }
                    break;
            }
        }

        public Task<int> GetAmountOfMessagesInExport(List<JsonFile> jsonFiles)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(jsonFiles);
            int count = 0;

            foreach (JsonFile file in jsonFiles)
            {
                count = +file.Conversation.Messages.Count;
            }
            return Task.FromResult(count);
        }
    }
}
