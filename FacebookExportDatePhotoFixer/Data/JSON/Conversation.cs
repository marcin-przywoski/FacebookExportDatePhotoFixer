using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FacebookExportDatePhotoFixer.Data.JSON
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
        public List<object>? MagicWords { get; set; }

        public class Message
        {
            [JsonProperty(PropertyName = "sender_name")]
            public string SenderName { get; set; }

            [Newtonsoft.Json.JsonConverter(typeof(MilisecondEpochConverter))]
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

            public class Photo
            {
                [JsonProperty(PropertyName = "uri")]
                public string Uri { get; set; }

                [Newtonsoft.Json.JsonConverter(typeof(MilisecondEpochConverter))]
                [JsonProperty(PropertyName = "creation_timestamp")]
                public DateTime CreationTimestamp { get; set; }
            }

            public class Video
            {
                [JsonProperty(PropertyName = "uri")]
                public string Uri { get; set; }

                [Newtonsoft.Json.JsonConverter(typeof(MilisecondEpochConverter))]
                [JsonProperty(PropertyName = "creation_timestamp")]
                public DateTime CreationTimestamp { get; set; }

            }

            public class Reaction
            {
                [JsonProperty(PropertyName = "reaction")]
                public string ReactionType { get; set; }

                [JsonProperty(PropertyName = "actor")]
                public string Actor { get; set; }
            }

            public class BumpedMessageMetadata
            {
                [JsonProperty(PropertyName = "bumped_message")]
                public string? BumpedMessage { get; set; }

                [JsonProperty(PropertyName = "is_bumped")]
                public bool? IsBumped { get; set; }
            }

            public class Gif
            {
                [JsonProperty(PropertyName = "uri")]
                public string Uri { get; set; }
            }
        }

        public class Participant
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

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
