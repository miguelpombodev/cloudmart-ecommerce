using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructureServices(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var checkConnectionString =
			configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("No Connection String informed");

		var connectionString = configuration.GetConnectionString(checkConnectionString);

		return services;
	}
}
