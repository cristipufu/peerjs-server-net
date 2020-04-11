using Newtonsoft.Json;

namespace PeerJs
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

        public static Message Error(string error)
        {
            return Create(MessageType.Error, error);
        }

        public static Message Create(string type, string msg)
        {
            return new Message
            {
                Type = type,
                Payload = new
                {
                    msg,
                }
            };
        }
    }
}