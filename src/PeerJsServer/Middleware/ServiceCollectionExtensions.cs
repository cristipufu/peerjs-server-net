using Microsoft.Extensions.DependencyInjection;
using PeerJsServer;

namespace Microsoft.AspNetCore.Builder
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPeerJsServer(this IServiceCollection services)
        {
            services.AddSingleton<IWebSocketServer, WebSocketServer>();
        }
    }
}