using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FacebookExportDatePhotoFixer.Data
{
    class HtmlFile
    {
        //private HtmlDocument htmlDocument;

        public string Location { get; }
        public int MessagesCount { get { return ListOfMessages.Count; } }
        public List<Message> ListOfMessages { get; } = new List<Message>();

        public HtmlFile(string path)
        {
            //this.htmlDocument.Load(path);
            this.Location = path;
        }
        
    }
}
