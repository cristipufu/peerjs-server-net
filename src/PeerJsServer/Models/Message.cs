using Newtonsoft.Json;

namespace PeerJsServer
{
    public class Message
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("dst")]
        public string Destination { get; set; }

        [JsonProperty("payload")]
        public dynamic Payload { get; set; }
    }
}