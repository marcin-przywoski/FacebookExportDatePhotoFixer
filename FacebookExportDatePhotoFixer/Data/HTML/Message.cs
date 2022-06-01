using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookExportDatePhotoFixer.Data.HTML
{
    class Message
    {
        //public string Sender { get; }

        public DateTime Date { get; }

        public string Link { get; }

        public string ExtensionOfAttachment { get; }

        public Message(DateTime date, string link)
        {
            //this.Sender = sender;
            Date = date;
            Link = link;
            ExtensionOfAttachment = link.Substring(link.Length - 4);
        }
    }
}
