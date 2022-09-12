using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacebookExportDatePhotoFixer.Data.JSON.Entities
{
    public class Gif
    {
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }
    }
}
