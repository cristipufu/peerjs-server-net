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
        public MessagePayload Payload { get; set; }
    }

    public class MessagePayload
    {
        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sdp")]
        public MessageSdp Sdp { get; set; }

        [JsonProperty("candidate")]
        public MessageCandidate Candidate { get; set; }

        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }

        [JsonProperty("browser")]
        public string Browser { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("serialization")]
        public string Serialization { get; set; }
    }

    public class MessageSdp
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sdp")]
        public string Sdp { get; set; }
    }

    public class MessageCandidate
    {
        [JsonProperty("candidate")]
        public string Candidate { get; set; }

        [JsonProperty("sdpMid")]
        public string SdpId { get; set; }

        [JsonProperty("sdpMLineIndex")]
        public string SdpLineIndex { get; set; }
    }
}
