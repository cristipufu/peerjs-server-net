using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace PeerJsServer
{
    public interface IMessageHandler
    {
        Task HandleAsync(IClient client, Message message);
    }

    public class MessageHandler : IMessageHandler
    {
        private readonly IRealm _realm;

        public MessageHandler(IRealm realm)
        {
            _realm = realm;
        }

        public Task HandleAsync(IClient client, Message message)
        {
            return message.Type switch
            {
                MessageType.Open => AcceptAsync(client),
                MessageType.Heartbeat => HeartbeatAsync(client),
                MessageType.Offer => TransferAsync(client, message),
                MessageType.Answer => TransferAsync(client, message),
                MessageType.Candidate => TransferAsync(client, message),
                MessageType.Expire => TransferAsync(client, message),
                MessageType.Leave => LeaveAsync(client, message),
                _ => throw new NotImplementedException(),
            };
        }

        private Task AcceptAsync(IClient client)
        {
            return client.SendAsync(new Message
            {
                Type = MessageType.Open,
            });
        }

        private Task HeartbeatAsync(IClient client)
        {
            client.SetLastHeartbeat(DateTime.UtcNow);

            return Task.CompletedTask;
        }

        private async Task TransferAsync(IClient client, Message message, CancellationToken cancellationToken = default)
        {
            var destinationClient = _realm.GetClient(message.Destination);

            if (destinationClient != null)
            {
                try
                {
                    message.Source = client.GetId();

                    await destinationClient.SendAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    var destinationSocket = destinationClient.GetSocket();

                    // This happens when a peer disconnects without closing connections and
                    // the associated WebSocket has not closed.
                    if (destinationSocket != null)
                    {
                        await destinationSocket.CloseAsync(WebSocketCloseStatus.Empty, ex.Message, cancellationToken);
                    }
                    else
                    {
                        _realm.RemoveClientById(message.Destination);
                    }

                    // Tell the other side to stop trying.
                    await LeaveAsync(destinationClient, new Message
                    {
                        Destination = message.Source,
                        Source = message.Destination,
                        Type = MessageType.Leave,
                    }, cancellationToken);
                }
            }
            else
            {
                if (message.ShouldQueue())
                {
                    _realm.AddMessageToQueue(message.Destination, message);
                }
            }
        }

        private async Task LeaveAsync(IClient client, Message message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(message.Destination))
            {
                _realm.RemoveClientById(message.Source);

                return;
            }

            await TransferAsync(client, message, cancellationToken);
        }
    }
}