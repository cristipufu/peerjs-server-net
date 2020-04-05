using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJsServer
{
    public interface IClient
    {
        string GetId();

        string GetToken();

        WebSocket GetSocket();

        DateTime GetLastHeartbeat();

        void SetLastHeartbeat(DateTime reportingTime);

        Task SendAsync(Message msg, CancellationToken cancellationToken = default);
    }

    public class Client : IClient
    {
        private readonly WebSocket _socket;
        private readonly ClientCredentials _credentials;

        private DateTime _reportingTime;

        public Client(ClientCredentials credentials, WebSocket socket)
        {
            _credentials = credentials;
            _socket = socket;
            _reportingTime = DateTime.UtcNow;
        }

        public string GetId()
        {
            return _credentials.ClientId;
        }

        public string GetToken()
        {
            return _credentials.Token;
        }

        public WebSocket GetSocket()
        {
            return _socket;
        }

        public DateTime GetLastHeartbeat()
        {
            return _reportingTime;
        }

        public void SetLastHeartbeat(DateTime reportingTime)
        {
            _reportingTime = reportingTime;
        }

        public async Task SendAsync(Message msg, CancellationToken cancellationToken = default)
        {
            if (_socket == null)
            {
                throw new Exception("Invalid client socket!");
            }

            var value = new ArraySegment<byte>(GetSerializedMessage(msg));

            await _socket.SendAsync(value, WebSocketMessageType.Text, true, cancellationToken);
        }

        private byte[] GetSerializedMessage(Message msg)
        {
            var serialized = JsonConvert.SerializeObject(msg,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });

            return Encoding.UTF8.GetBytes(serialized);
        }
    }
}
