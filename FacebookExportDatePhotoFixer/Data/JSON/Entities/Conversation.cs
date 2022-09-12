using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FacebookExportDatePhotoFixer.Data.JSON.Entities
{
    [JsonObject("Root")]
    public class Conversation
    {
        [JsonProperty(PropertyName = "participants")]
        public List<Participant> Participants { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "is_still_participant")]
        public bool IsStillParticipant { get; set; }

        [JsonProperty(PropertyName = "thread_type")]
        public string ThreadType { get; set; }

        [JsonProperty(PropertyName = "thread_path")]
        public string ThreadPath { get; set; }

        [JsonProperty(PropertyName = "magic_words")]
        public List<object> MagicWords { get; set; }

    }

    public class MilisecondEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds + "");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            return _epoch.AddMilliseconds((long)reader.Value / 1d);
        }
    }
}
