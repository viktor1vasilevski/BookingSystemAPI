using Data.Clients;
using Main.Interfaces;
using Main.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IoC;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpClient<ITravelApiClient, TravelApiClient>();

        return services;
    }
}
