using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJs
{
    public interface IPeerJsServer
    {
        Task RegisterClientAsync(IClientCredentals credentials, WebSocket socket, TaskCompletionSource<object> requestCompletedTcs, CancellationToken cancellationToken = default);
    }

    public class PeerJsServer : IPeerJsServer
    {
        private readonly IRealm _realm;

        public PeerJsServer()
        {
            _realm = new Realm();
        }

        public async Task RegisterClientAsync(IClientCredentals credentials, WebSocket socket, TaskCompletionSource<object> requestCompletedTcs, CancellationToken cancellationToken = default)
        {
            if (!credentials.Valid)
            {
                await socket.SendMessageAsync(Message.Error(Errors.InvalidWsParameters), cancellationToken);

                await socket.CloseAsync(Errors.InvalidWsParameters);

                requestCompletedTcs.TrySetResult(null);

                return;
            }

            var client = _realm.GetClient(credentials.ClientId);

            if (client != null)
            {
                if (credentials.Token != client.GetToken())
                {
                    await socket.SendMessageAsync(Message.Create(MessageType.IdTaken, "Id is already used!"));

                    await socket.CloseAsync(Errors.InvalidToken);

                    requestCompletedTcs.TrySetResult(null);

                    return;
                }

                client.SetSocket(socket);

                // TODO send queued messages

            }
            else
            {
                // TODO check concurrent limit options

                client = new Client(credentials, socket);

                _realm.SetClient(client);

                await client.SendAsync(new Message
                {
                    Type = MessageType.Open,
                }, cancellationToken);
            }

            // listen for incoming messages
            await AwaitReceiveAsync(client, cancellationToken);

            // clean-up after socket close
            _realm.RemoveClientById(client.GetId());

            requestCompletedTcs.TrySetResult(null);
        }

        private async Task AwaitReceiveAsync(IClient client, CancellationToken cancellationToken = default)
        {
            var socket = client.GetSocket();

            var buffer = new ArraySegment<byte>(new byte[1024 * 16]);

            WebSocketReceiveResult result;

            do
            {
                var (readResult, message) = await ReadAsync(socket, buffer, cancellationToken);

                await HandleMessageAsync(client, message, cancellationToken);

                result = readResult;
            }
            while (!result.CloseStatus.HasValue);

            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken);
        }

        private async Task<(WebSocketReceiveResult, string)> ReadAsync(WebSocket socket, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
        {
            WebSocketReceiveResult result;

            using var ms = new MemoryStream();

            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);

                ms.Write(buffer.Array, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                using var reader = new StreamReader(ms, Encoding.UTF8);

                return (result, reader.ReadToEnd());
            }

            return (result, string.Empty);
        }

        private async Task HandleMessageAsync(IClient client, string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var message = JsonConvert.DeserializeObject<Message>(text);

            await _realm.HandleMessageAsync(client, message, cancellationToken);
        }
    }
}