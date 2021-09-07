using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using HtmlAgilityPack;

namespace FacebookExportDatePhotoFixer.Data
{
    class FacebookExport
    {
        public List<HtmlFile> HtmlList { get; } = new List<HtmlFile>();

        public string Location { get; set; }

        public string Destination { get; set; }
        public CultureInfo Language { get; set; }

        public void GetLanguage()
        {
            if(File.Exists(this.Location + "/about_you/preferences.html"))
            {
                string preferencesLocation = this.Location + "/about_you/preferences.html";
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.Load(preferencesLocation);
                string locale = htmlDocument.DocumentNode.SelectSingleNode("/ html / body / div / div / div / div[2] / div[2] / div / div[3] / div / div[2] / div[1] / div[2] / div / div / div / div[1] / div[3]").InnerText;
                this.Language = new CultureInfo(locale, false);
            }
            else if(File.Exists(this.Location + "/preferences/language_and_locale.html"))
            {
                string preferencesLocation = this.Location + "/preferences/language_and_locale.html";
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.Load(preferencesLocation);
                string locale = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div/div/div/div[2]/div[2]/div/div[1]/div/div[2]/div[1]/div[2]/div/div/div/div[1]/div[3]").InnerText;
                this.Language = new CultureInfo(locale, false);
            }

        }

        public void GetHtmlFiles(ProgressBar progressBar, ListBox outputLog)
        {
            outputLog.Dispatcher.Invoke(() =>
            {
                outputLog.Items.Add("Export language : " +this.Language);
                outputLog.SelectedIndex = outputLog.Items.Count - 1;
                outputLog.ScrollIntoView(outputLog.SelectedItem);
            });

            List<string> listOfHtml = new List<string>();
            string messagesLocation = this.Location + "/messages/archived_threads/";
            outputLog.Dispatcher.Invoke(() =>
            {   
                outputLog.Items.Add("Gathering HTML files from archived threads...");
                outputLog.SelectedIndex = outputLog.Items.Count - 1;
                outputLog.ScrollIntoView(outputLog.SelectedItem);
            });
            listOfHtml = Directory.GetFiles(messagesLocation, "*.html", SearchOption.AllDirectories).ToList();
            outputLog.Dispatcher.Invoke(() =>
            {
                outputLog.Items.Add("Gathering HTML files from filtered threads...");
                outputLog.SelectedIndex = outputLog.Items.Count - 1;
                outputLog.ScrollIntoView(outputLog.SelectedItem);
            });
            messagesLocation = Location + "/messages/filtered_threads/";
            listOfHtml.AddRange(Directory.GetFiles(messagesLocation, "*.html", SearchOption.AllDirectories).ToList());
            outputLog.Dispatcher.Invoke(() =>
            {
                outputLog.Items.Add("Gathering HTML files from inbox...");
                outputLog.SelectedIndex = outputLog.Items.Count - 1;
                outputLog.ScrollIntoView(outputLog.SelectedItem);
            });
            messagesLocation = Location + "/messages/inbox/";
            listOfHtml.AddRange(Directory.GetFiles(messagesLocation, "*.html", SearchOption.AllDirectories).ToList());

            foreach (string htmlFile in listOfHtml)
            {
                HtmlList.Add(new HtmlFile(htmlFile));
            }
            outputLog.Dispatcher.Invoke(() =>
            {
                outputLog.Items.Add("Found " + HtmlList.Count + " HTML files to process");
                outputLog.SelectedIndex = outputLog.Items.Count - 1;
                progressBar.Maximum = HtmlList.Count;
                outputLog.ScrollIntoView(outputLog.SelectedItem);
            });
        }

        public void GetMessagesFromHtmlFiles(ProgressBar progressBar, ListBox outputLog)
        {
            // CultureInfo
            //     System.Globalization.RegionInfo

            foreach (HtmlFile file in HtmlList)
            {

            
            outputLog.Dispatcher.InvokeAsync(() =>
            {
                progressBar.Value++;
                outputLog.SelectedIndex = outputLog.Items.Count - 1;
                outputLog.ScrollIntoView(outputLog.SelectedItem);
            });

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.Load(file.Location);
            HtmlNodeCollection divs = htmlDocument.DocumentNode.SelectNodes("//div[@class='pam _3-95 _2pi0 _2lej uiBoxWhite noborder']");

            foreach (HtmlNode node in divs)
            {
                if (node.SelectSingleNode(".//a[@href]") != null)
                {
                    string href = node.SelectSingleNode(".//a[@href]").GetAttributeValue("href", string.Empty);

                    if (!(href.StartsWith("http")) || !(href.StartsWith("https")))
                    {
                        if (href.EndsWith(".jpg") || href.EndsWith(".png") || href.EndsWith(".gif") || href.EndsWith(".mp4"))
                        {

                            DateTime date = Convert.ToDateTime(node.SelectSingleNode(".//div[@class='_3-94 _2lem']").InnerText,this.Language);

                            HtmlNode link = node.SelectSingleNode(".//a[@href]");
                            //string href = link.GetAttributeValue("href", string.Empty);

                            if (File.Exists(this.Location + href))
                            {
                                    file.ListOfMessages.Add(new Message(date, href));
                                }


                        }
                    }
                }
            }
            outputLog.Dispatcher.Invoke(() =>
            {
                if(file.MessagesCount != 0) 
                {
                    outputLog.Items.Add($"There is {file.MessagesCount} of messages with linked media");
                    outputLog.SelectedIndex = outputLog.Items.Count - 1;
                }

                outputLog.ScrollIntoView(outputLog.SelectedItem);
            });
                
            }
            this.HtmlList.RemoveAll(s => s.ListOfMessages.Count == 0);
        }

        public void ProcessHtmlFiles(ProgressBar progressBar, ListBox outputLog, CheckBox changeNameCheckbox)
        {
            outputLog.Dispatcher.InvokeAsync(() =>
            {
                progressBar.Value = 0;
                progressBar.Maximum = GetAmountOfMessagesInExport(HtmlList);
            });

            foreach (HtmlFile file in HtmlList)
            {
                foreach (Message message in file.ListOfMessages)
                {
                    progressBar.Dispatcher.Invoke(() =>
                    {
                        progressBar.Value++;
                    });

                    try
                    {
                        if(changeNameCheckbox.IsChecked != false) 
                        {
                            if (File.Exists(this.Location + message.Link))
                            {
                                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Destination + message.Link));
                                File.Copy(this.Location + message.Link, this.Destination + message.Link);
                                FileInfo fileInfo = new FileInfo(this.Destination + message.Link);
                                string newName = fileInfo.DirectoryName + message.Date;
                                File.Move(this.Destination + message.Link, newName);
                                File.SetCreationTime(newName, message.Date);
                                File.SetLastAccessTime(newName, message.Date);
                                File.SetLastWriteTime(newName, message.Date);
                            }
                        }
                        else 
                        {
                            if (File.Exists(this.Location + message.Link))
                            {
                                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Destination + message.Link));
                                File.Copy(this.Location + message.Link, this.Destination + message.Link);
                                File.SetCreationTime(this.Destination + message.Link, message.Date);
                                File.SetLastAccessTime(this.Destination + message.Link, message.Date);
                                File.SetLastWriteTime(this.Destination + message.Link, message.Date);
                            }
                        }
                    }
                    catch (IOException)
                    {

                        outputLog.Dispatcher.Invoke(() =>
                        {
                            outputLog.Items.Add($" {this.Location + message.Link} already exists at destination folder");
                            outputLog.SelectedIndex = outputLog.Items.Count - 1;
                            outputLog.ScrollIntoView(outputLog.SelectedItem);
                        });
                    }

                }
            }
        }

        private int GetAmountOfMessagesInExport(List<HtmlFile> htmlFiles)
        {
            int count = 0;

            foreach (HtmlFile file in htmlFiles)
            {
                count = +file.MessagesCount;
            }
            return count;
        }
    }
}
