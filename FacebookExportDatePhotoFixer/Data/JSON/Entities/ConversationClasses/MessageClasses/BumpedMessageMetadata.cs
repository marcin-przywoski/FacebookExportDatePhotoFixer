using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacebookExportDatePhotoFixer.Data.JSON.Entities
{
    public class BumpedMessageMetadata
    {
        [JsonProperty(PropertyName = "bumped_message")]
        public string BumpedMessage { get; set; }

        [JsonProperty(PropertyName = "is_bumped")]
        public bool? IsBumped { get; set; }
    }
}
