using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJsServer
{
    public interface IWebSocketServer
    {
        Task RegisterClientAsync(IClient client, CancellationToken cancellationToken = default);
    }

    public class WebSocketServer : IWebSocketServer
    {
        private readonly IRealm _realm;
        private readonly IMessageHandler _messageHandler;

        public WebSocketServer()
        {
            _realm = new Realm();
            _messageHandler = new MessageHandler(_realm);
        }

        public async Task RegisterClientAsync(IClient client, CancellationToken cancellationToken = default)
        {
            _realm.SetClient(client);

            await client.SendAsync(new Message
            {
                Type = MessageType.Open,
            }, cancellationToken);

            await ListenAsync(client, cancellationToken);
        }

        private async Task ListenAsync(IClient client, CancellationToken cancellationToken = default)
        {
            var socket = client.GetSocket();
            var buffer = new ArraySegment<byte>(new byte[1024 * 16]);
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                using var ms = new MemoryStream();

                ms.Write(buffer.Array, buffer.Offset, result.Count);

                ms.Seek(0, SeekOrigin.Begin);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    using var reader = new StreamReader(ms, Encoding.UTF8);

                    var text = reader.ReadToEnd();

                    await HandleMessageAsync(client, text);
                }

                result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            }

            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

            //tcs.TrySetResult(null);
        }

        private async Task HandleMessageAsync(IClient client, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var message = JsonConvert.DeserializeObject<Message>(text);

            await _messageHandler.HandleAsync(client, message);
        }
    }
}
