using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Classes
{
    public class Word
    {
        [JsonProperty(Required = Required.Always)]
        public string WordText { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Translation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Transcription { get; set; }
    }
}
