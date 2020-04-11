using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJs
{
    public static class WebSocketExtensions
    {
        public static Task SendMessageAsync(this WebSocket socket, Message msg, CancellationToken cancellationToken = default)
        {
            var value = new ArraySegment<byte>(GetSerializedMessage(msg));

            return socket.SendAsync(value, WebSocketMessageType.Text, true, cancellationToken);
        }

        public static Task CloseAsync(this WebSocket socket, string description)
        {
            if (socket == null)
            {
                return Task.CompletedTask;
            }

            return socket.CloseAsync(WebSocketCloseStatus.Empty, description, CancellationToken.None);
        }

        private static byte[] GetSerializedMessage(Message msg)
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