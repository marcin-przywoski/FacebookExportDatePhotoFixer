using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FacebookExportDatePhotoFixer.Data.JSON.Entities
{ 
public class Message
{
    [JsonProperty(PropertyName = "sender_name")]
    public string SenderName { get; set; }

    [JsonConverter(typeof(MilisecondEpochConverter))]
    [JsonProperty(PropertyName = "timestamp_ms")]
    public DateTime Date { get; set; }

    [JsonProperty(PropertyName = "content")]
    public string Content { get; set; }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }

    [JsonProperty(PropertyName = "is_unsent")]
    public bool IsUnsent { get; set; }

    [JsonProperty(PropertyName = "is_taken_down")]
    public bool IsTakenDown { get; set; }

    [JsonProperty(PropertyName = "bumped_message_metadata")]
    public BumpedMessageMetadata BumpedMessageMeta { get; set; }

    [JsonProperty(PropertyName = "gifs")]
    public List<Gif> Gifs { get; set; }

    [JsonProperty(PropertyName = "reactions")]
    public List<Reaction> Reactions { get; set; }

    [JsonProperty(PropertyName = "photos")]
    public List<Photo> Photos { get; set; }

    [JsonProperty(PropertyName = "videos")]
    public List<Video> Videos { get; set; }
}
}