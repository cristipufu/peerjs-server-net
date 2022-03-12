using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace PeerJs
{
    public class PeerJsMiddleware
    {
        private readonly ILogger<PeerJsMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IPeerJsServer _webSocketServer;

        public PeerJsMiddleware(
            RequestDelegate next,
            IPeerJsServer webSocketServer,
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
                // TODO API: handle xhr requests
                context.Response.StatusCode = 200;

                return;
            }

            WebSocket socket = null;

            try
            {
                var requestCompletedTcs = new TaskCompletionSource<object>();

                socket = await context.WebSockets.AcceptWebSocketAsync();

                var credentials = GetCredentials(context.Request.Query);

                await _webSocketServer.RegisterClientAsync(credentials, socket, requestCompletedTcs, context.RequestAborted);

                await requestCompletedTcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{}", ex.Message);

                await socket.CloseAsync(ex.Message);
            }
            finally
            {
                socket?.Dispose();
            }
        }

        private static IClientCredentals GetCredentials(IQueryCollection queryString)
        {
            return new ClientCredentials(
                clientId: GetQueryStringValue(queryString, "id"),
                token: GetQueryStringValue(queryString, "token"),
                key: GetQueryStringValue(queryString, "key"));
        }

        private static string GetQueryStringValue(IQueryCollection queryString, string key)
        {
            return queryString.TryGetValue(key, out var value) ? value.ToString() : string.Empty;
        }
    }
}