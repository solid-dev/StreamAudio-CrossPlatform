using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamAudio.WP.Models
{
    public class AudioModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("audio_link")]
        public string PathMp3 { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("singer")]
        public string Singer { get; set; }
    }
}
