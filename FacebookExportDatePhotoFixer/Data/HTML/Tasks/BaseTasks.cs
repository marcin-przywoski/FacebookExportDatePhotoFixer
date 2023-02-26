using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using AngleSharp.XPath;
using AngleSharp.Html.Dom;

namespace FacebookExportDatePhotoFixer.Data.HTML
{
    partial class HtmlExport
    {
        private async Task GetMessages(HtmlFile html)
        {
            outputLogSubject.OnNext($"Getting messeges from: {html.Location}" + "\n");

            string source = await File.ReadAllTextAsync(html.Location);
            var parser = new HtmlParser();
            var document = parser.ParseDocument(source);
            var divs = document.Body.SelectNodes("//div[@class='pam _3-95 _2pi0 _2lej uiBoxWhite noborder']");

            foreach (INode node in divs)
            {
                if ((node.Descendents().OfType<IHtmlDivElement>().First(x => x.ClassList.Contains("_3-94") && x.ClassList.Contains("_2lem")).TextContent != "" ))
                {
                    if (node.Descendents().OfType<IHtmlAnchorElement>().Any())
                    {
                        if ((node.Descendents().OfType<IHtmlAnchorElement>().First(x => x.HasAttribute("href"))).GetAttribute("href") != "")
                        {
                            string href = (node.Descendents().OfType<IHtmlAnchorElement>().First(x => x.HasAttribute("href"))).GetAttribute("href");

                            if (!href.StartsWith("http") || !href.StartsWith("https"))
                            {
                                if (href.EndsWith(".jpg") || href.EndsWith(".png") || href.EndsWith(".gif") || href.EndsWith(".mp4"))
                                {
                                    string divDate = node.Descendents().OfType<IHtmlDivElement>().First(x => x.ClassList.Contains("_3-94") && x.ClassList.Contains("_2lem")).TextContent;

                                    DateTime date = Convert.ToDateTime(divDate, Language);

                                    if (File.Exists(Location + href))
                                    {
                                        html.ListOfMessages.Add(new Message(date, href));
                                    }
                                }
                            }
                        }
                    }

                }
            }
            if (html.ListOfMessages.Count != 0)
            {
                outputLogSubject.OnNext($"There is {html.MessagesCount} of messages with linked media" + "\n");
            }
        }



        private async Task ProcessHtml(HtmlFile html, CheckBox checkbox, ConcurrentBag<string> outputLogUpdate)
        {
            bool? isChecked = checkbox.Dispatcher.Invoke(() => checkbox.IsChecked);

            List<int> numTasks = new List<int>();
            numTasks.AddRange(Enumerable.Range(0, html.ListOfMessages.Count).ToList());

            int _coresCount = Environment.ProcessorCount;
            var tasks = new List<Task>();
            var queue = new ConcurrentQueue<int>(numTasks);
            for (int i = 0; i < _coresCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (queue.TryDequeue(out int number))
                    {
                        await ProcessMessage(html.ListOfMessages[number], isChecked, outputLogUpdate);
                    }
                }));
            }
            await Task.WhenAll(tasks);

            string update = string.Join("", outputLogUpdate);
            outputLogSubject.OnNext(update);
        }

        private Task ProcessMessage(Message message, bool? isChecked, ConcurrentBag<string> outputLogUpdate)
        {
            outputLogUpdate.Add($"Processing: {message.Link}" + "\n");

            try
            {
                if (isChecked == true)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Destination + message.Link));
                    string date = message.Date.ToString("yyyyMMdd_HHmmss");
                    string newName = message.Link.Replace(Path.GetFileNameWithoutExtension(message.Link), date);
                    File.Copy(Location + message.Link, Destination + newName);
                    File.SetCreationTime(Destination + newName, message.Date);
                    File.SetLastAccessTime(Destination + newName, message.Date);
                    File.SetLastWriteTime(Destination + newName, message.Date);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Destination + message.Link));
                    File.Copy(Location + message.Link, Destination + message.Link);
                    File.SetCreationTime(Destination + message.Link, message.Date);
                    File.SetLastAccessTime(Destination + message.Link, message.Date);
                    File.SetLastWriteTime(Destination + message.Link, message.Date);
                }
            }
            catch (IOException e)
            {
                if (message.Link.Contains("stickers_used"))
                {
                    outputLogUpdate.Add($" {Path.GetFileNameWithoutExtension(message.Link)} is a sticker, skipping" + "\n");
                }
                else
                {
                    DateTime dateNewName = message.Date;
                    string date = dateNewName.ToString("yyyyMMdd_HHmmss");
                    string newNameException = message.Link.Replace(Path.GetFileNameWithoutExtension(message.Link), date);

                    while (File.Exists(Destination + newNameException))
                    {
                        outputLogUpdate.Add($" {Path.GetFileNameWithoutExtension(message.Link)} file with same date as name already exists at target location!" + "\n");
                        outputLogUpdate.Add("Adding 1 second to filename to avoid I/O conflicts" + "\n");

                        dateNewName = dateNewName.AddSeconds(1);
                        string dateFixed = dateNewName.ToString("yyyyMMdd_HHmmss");
                        newNameException = message.Link.Replace(Path.GetFileNameWithoutExtension(message.Link), dateFixed);
                    }
                    File.Copy(Location + message.Link, Destination + newNameException);
                    File.SetCreationTime(Destination + newNameException, message.Date);
                    File.SetLastAccessTime(Destination + newNameException, message.Date);
                    File.SetLastWriteTime(Destination + newNameException, message.Date);
                }
            }

            return Task.CompletedTask;
        }
    }
}
