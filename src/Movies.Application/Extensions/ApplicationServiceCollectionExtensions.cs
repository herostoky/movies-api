using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Databases;
using Movies.Application.Repositories;

namespace Movies.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository, MovieRepository>();
        return services;
    }

    public static IServiceCollection AddDatabases(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new NpgSqlConnectionFactory(connectionString));
        services.AddSingleton<DbInitializer>();
        return services;
    }
}