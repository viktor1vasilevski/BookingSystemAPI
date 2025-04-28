using Main.Interfaces;
using Main.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpClient<IManagerService, ManagerService>();

        return services;
    }
}
