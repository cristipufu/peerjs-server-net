namespace PeerJs
{
    public static class Errors
    {
        public const string InvalidKey = "Invalid key provided";
        public const string InvalidToken = "Invalid token provided";
        public const string InvalidWsParameters = "No id, token, or key supplied to websocket server";
        public const string ConnectionLimitExceeded = "Server has reached its concurrent user limit";
    }

    public static class MessageType
    {
        public const string Heartbeat = "HEARTBEAT";
        public const string Candidate = "CANDIDATE";
        public const string Offer = "OFFER";
        public const string Answer = "ANSWER";
        public const string Open = "OPEN"; // The connection to the server is open.
        public const string Error = "ERROR"; // Server error.
        public const string IdTaken = "ID-TAKEN"; // The selected ID is taken.
        public const string InvalidKey = "INVALID-KEY"; // The given API key cannot be found.
        public const string Leave = "LEAVE"; // Another peer has closed its connection to this peer.
        public const string Expire = "EXPIRE"; // The offer sent to a peer has expired without response.
        public const string IsAvailable = "IS_AVAILABLE"; // Request the availability for a specified peer

        public static bool ShouldQueue(this Message message)
        {
            return message.Type != Leave && message.Type != Expire && !string.IsNullOrEmpty(message.Destination);
        }
    }
}
