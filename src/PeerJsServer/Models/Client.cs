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

        void SetSocket(WebSocket socket);

        DateTime GetLastHeartbeat();

        void SetLastHeartbeat(DateTime reportingTime);

        Task SendAsync(Message msg, CancellationToken cancellationToken = default);
    }

    public class Client : IClient
    {
        private readonly string _id;
        private readonly string _token;
        private WebSocket _socket;
        private DateTime _reportingTime;

        public Client(string id, string token)
        {
            _id = id;
            _token = token;
            _reportingTime = DateTime.UtcNow;
        }

        public string GetId()
        {
            return _id;
        }

        public string GetToken()
        {
            return _token;
        }

        public WebSocket GetSocket()
        {
            return _socket;
        }

        public void SetSocket(WebSocket socket)
        {
            _socket = socket;
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
