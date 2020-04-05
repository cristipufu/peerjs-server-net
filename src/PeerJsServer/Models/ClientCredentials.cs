namespace PeerJsServer
{
    public interface IClientCredentals
    {
        string ClientId { get; }

        string Token { get; }

        string Key { get; }
    }

    public class ClientCredentials : IClientCredentals
    {
        public ClientCredentials(string clientId, string token, string key)
        {
            ClientId = clientId;
            Token = token;
            Key = key;
        }

        public string ClientId { get; }

        public string Token { get; }

        public string Key { get; }

        public bool Valid =>
            !string.IsNullOrEmpty(ClientId) &&
            !string.IsNullOrEmpty(Token) &&
            !string.IsNullOrEmpty(Key);
    }
}
