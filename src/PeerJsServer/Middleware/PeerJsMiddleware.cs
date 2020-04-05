using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJsServer
{
    public class PeerJsMiddleware
    {
        private readonly ILogger<PeerJsMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IWebSocketServer _webSocketServer;

        public PeerJsMiddleware(
            RequestDelegate next,
            IWebSocketServer webSocketServer,
            ILogger<PeerJsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _webSocketServer = webSocketServer;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToString();

            if (!path.Contains("/peerjs"))
            {
                await _next.Invoke(context);

                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 200;

                return;
            }

            WebSocket socket = null;

            try
            {
                var requestCompletedTcs = new TaskCompletionSource<object>();

                socket = await context.WebSockets.AcceptWebSocketAsync();

                var queryString = context.Request.Query;

                var credentials = new ClientCredentials(
                    clientId: GetQueryStringValue(queryString, "id"),
                    token: GetQueryStringValue(queryString, "token"),
                    key: GetQueryStringValue(queryString, "key"));

                var client = new Client(credentials, socket);

                if (!credentials.Valid)
                {
                    await client.SendAsync(Message.Error(Errors.InvalidWsParameters));

                    await CloseConnectionAsync(socket, Errors.InvalidWsParameters);
                }

                await _webSocketServer.RegisterClientAsync(client, requestCompletedTcs, context.RequestAborted);

                await requestCompletedTcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                await CloseConnectionAsync(socket, ex.Message);
            }
        }

        private async Task CloseConnectionAsync(WebSocket socket, string description)
        {
            if (socket != null)
            {
                await socket.CloseAsync(WebSocketCloseStatus.Empty, description, CancellationToken.None);
            }
        }

        private static string GetQueryStringValue(IQueryCollection queryString, string key)
        {
            return queryString.TryGetValue(key, out var value) ? value.ToString() : string.Empty;
        }
    }
}
