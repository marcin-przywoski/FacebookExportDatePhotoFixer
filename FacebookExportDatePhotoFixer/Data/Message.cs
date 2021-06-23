using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacebookExportDatePhotoFixer.Data
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
            this.Date = date;
            this.Link = link;
            this.ExtensionOfAttachment = link.Substring(link.Length - 4);
        }
    }
}
