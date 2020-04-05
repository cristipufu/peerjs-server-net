using Microsoft.Extensions.DependencyInjection;

namespace PeerJsServer
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPeerJsServer(this IServiceCollection services)
        {
            services.AddSingleton<IWebSocketServer, WebSocketServer>();
        }
    }
}
