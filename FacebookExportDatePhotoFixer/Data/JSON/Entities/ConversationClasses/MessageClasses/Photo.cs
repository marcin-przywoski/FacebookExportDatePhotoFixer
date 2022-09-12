using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacebookExportDatePhotoFixer.Data.JSON.Entities
{
    public class Photo
    {
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        [JsonConverter(typeof(MilisecondEpochConverter))]
        [JsonProperty(PropertyName = "creation_timestamp")]
        public DateTime CreationTimestamp { get; set; }
    }
}
