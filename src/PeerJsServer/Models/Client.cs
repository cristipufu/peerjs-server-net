using System;
using System.Net.WebSockets;
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
        private readonly IClientCredentals _credentials;

        private WebSocket _socket;
        private DateTime _reportingTime;

        public Client(IClientCredentals credentials, WebSocket socket)
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

        public Task SendAsync(Message msg, CancellationToken cancellationToken = default)
        {
            if (_socket == null)
            {
                throw new Exception("Invalid client socket!");
            }

            return WebSocketServer.SendMessageAsync(_socket, msg, cancellationToken);
        }
    }
}
