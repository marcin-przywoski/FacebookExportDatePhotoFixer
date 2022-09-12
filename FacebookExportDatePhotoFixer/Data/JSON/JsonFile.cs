using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FacebookExportDatePhotoFixer.Data.JSON.Entities;
using FacebookExportDatePhotoFixer.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FacebookExportDatePhotoFixer.Data.JSON
{
    public class JsonFile
    {
        public string Location { get; }

        public Conversation Conversation { get; set; }

        public int TotalMessagesCount { get { return Conversation.Messages.Count; } }

        public JsonFile(string path)
        {
            Location = path;
        }

    }
}
