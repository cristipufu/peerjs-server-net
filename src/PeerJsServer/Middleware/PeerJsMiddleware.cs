using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
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

            var queryString = context.Request.Query;

            var id = queryString["id"];
            var token = queryString["token"];
            //var key = queryString["key"];

            try
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var requestCompletedTcs = new TaskCompletionSource<object>();

                var client = new Client(id, token);
                client.SetSocket(socket);

                await _webSocketServer.RegisterClientAsync(client, requestCompletedTcs, context.RequestAborted);

                await requestCompletedTcs.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
