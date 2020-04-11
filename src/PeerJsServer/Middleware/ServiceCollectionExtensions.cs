using Microsoft.Extensions.DependencyInjection;
using PeerJs;

namespace Microsoft.AspNetCore.Builder
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPeerJsServer(this IServiceCollection services)
        {
            services.AddSingleton<IPeerJsServer, PeerJsServer>();
        }
    }
}