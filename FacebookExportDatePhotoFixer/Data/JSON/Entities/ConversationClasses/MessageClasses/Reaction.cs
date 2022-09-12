using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacebookExportDatePhotoFixer.Data.JSON.Entities
{
    public class Reaction
    {
        [JsonProperty(PropertyName = "reaction")]
        public string ReactionType { get; set; }

        [JsonProperty(PropertyName = "actor")]
        public string Actor { get; set; }
    }
}
