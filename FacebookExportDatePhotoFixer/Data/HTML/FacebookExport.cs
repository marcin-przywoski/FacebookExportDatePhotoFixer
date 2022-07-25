using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using HtmlAgilityPack;

namespace FacebookExportDatePhotoFixer.Data.HTML
{
    class FacebookExport
    {
        public delegate Task ProgressUpdate(string value);

        public delegate Task BarUpdate(int value);

        public event ProgressUpdate OnProgressUpdateList;

        public event BarUpdate OnProgressUpdateBar;
        public List<HtmlFile> HtmlList { get; } = new List<HtmlFile>();

        public string Location { get; set; }

        public string Destination { get; set; }

        public CultureInfo Language { get; set; }

        public FacebookExport(string location, string destination)
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

        public async Task GetHtmlFiles()
        {
            await Task.Run(async () =>
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
            });

        }

        public async Task GetMessagesFromHtmlFiles()
        {
            await Task.Run(async () =>
             {
                 if (OnProgressUpdateBar != null)
                 {
                     OnProgressUpdateBar(HtmlList.Count);
                 }

                 foreach (HtmlFile file in HtmlList)
                 {
                     if (OnProgressUpdateList != null)
                     {
                         OnProgressUpdateList("Processing : " + file.Location);
                     }

                     if (OnProgressUpdateBar != null)
                     {
                         OnProgressUpdateBar(1);
                     }

                     HtmlDocument htmlDocument = new HtmlDocument();
                     htmlDocument.Load(file.Location);
                     HtmlNodeCollection divs = htmlDocument.DocumentNode.SelectNodes("//div[@class='pam _3-95 _2pi0 _2lej uiBoxWhite noborder']");

                     foreach (HtmlNode node in divs)
                     {
                         if (node.SelectSingleNode(".//div[@class='_3-94 _2lem']").InnerText != "")
                         {


                             if (node.SelectSingleNode(".//a[@href]") != null)
                             {
                                 string href = node.SelectSingleNode(".//a[@href]").GetAttributeValue("href", string.Empty);

                                 if (!href.StartsWith("http") || !href.StartsWith("https"))
                                 {
                                     if (href.EndsWith(".jpg") || href.EndsWith(".png") || href.EndsWith(".gif") || href.EndsWith(".mp4"))
                                     {

                                         DateTime date = Convert.ToDateTime(node.SelectSingleNode(".//div[@class='_3-94 _2lem']").InnerText, Language);

                                         HtmlNode link = node.SelectSingleNode(".//a[@href]");

                                         if (File.Exists(Location + href))
                                         {
                                             file.ListOfMessages.Add(new Message(date, href));
                                         }


                                     }
                                 }
                             }
                         }
                     }
                     if (OnProgressUpdateList != null)
                     {
                         if (file.MessagesCount != 0)
                         {
                             OnProgressUpdateList($"There is {file.MessagesCount} of messages with linked media");
                         }
                     }

                 }
                 HtmlList.RemoveAll(s => s.ListOfMessages.Count == 0);
             });

        }

        public async Task ProcessHtmlFiles(CheckBox changeNameCheckbox)
        {
            await Task.Run(async () =>
            {
                if (OnProgressUpdateBar != null)
                {
                    OnProgressUpdateBar(0);
                    OnProgressUpdateBar(await GetAmountOfMessagesInExport(HtmlList));
                }

                foreach (HtmlFile file in HtmlList)
                {
                    foreach (Message message in file.ListOfMessages)
                    {
                        if (OnProgressUpdateBar != null)
                        {
                            OnProgressUpdateBar(1);
                        }

                        bool? isChecked = changeNameCheckbox.Dispatcher.Invoke(() => changeNameCheckbox.IsChecked);

                        try
                        {
                            if (isChecked == true)
                            {
                                if (File.Exists(Location + message.Link))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(Destination + message.Link));
                                    string date = message.Date.ToString("yyyyMMdd_HHmmss");
                                    string newName = message.Link.Replace(Path.GetFileNameWithoutExtension(message.Link), date);
                                    File.Copy(Location + message.Link, Destination + newName);
                                    File.SetCreationTime(Destination + newName, message.Date);
                                    File.SetLastAccessTime(Destination + newName, message.Date);
                                    File.SetLastWriteTime(Destination + newName, message.Date);
                                }
                            }
                            else
                            {
                                if (File.Exists(Location + message.Link))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(Destination + message.Link));
                                    File.Copy(Location + message.Link, Destination + message.Link);
                                    File.SetCreationTime(Destination + message.Link, message.Date);
                                    File.SetLastAccessTime(Destination + message.Link, message.Date);
                                    File.SetLastWriteTime(Destination + message.Link, message.Date);
                                }
                            }
                        }
                        catch (IOException e)
                        {
                            if (message.Link.Contains("stickers_used"))
                            {
                                if (OnProgressUpdateList != null)
                                {
                                    OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Link)} is a sticker, skipping");
                                }
                            }
                            else
                            {
                                DateTime dateNewName = message.Date;
                                string date = dateNewName.ToString("yyyyMMdd_HHmmss");
                                string newNameException = message.Link.Replace(Path.GetFileNameWithoutExtension(message.Link), date);

                                while (File.Exists(Destination + newNameException))
                                {
                                    if (OnProgressUpdateList != null)
                                    {
                                        OnProgressUpdateList($" {Path.GetFileNameWithoutExtension(message.Link)} file with same date as name already exists at target location!");
                                        OnProgressUpdateList("Adding 1 second to filename to avoid I/O conflicts");
                                    }
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

                    }
                }
                if (OnProgressUpdateList != null)
                {
                    {
                        OnProgressUpdateList("Done!");
                    }
                }
            });

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
