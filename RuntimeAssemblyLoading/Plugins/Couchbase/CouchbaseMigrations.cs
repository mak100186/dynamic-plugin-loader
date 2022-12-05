using CouchbasePlugin.Migrations;

using DnsClient.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Unibet.Infrastructure.Caching.CouchbaseV7.Migrations.Abstractions;

namespace CouchbasePlugin;
static class CouchbaseMigrations
{
    public static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Main>>();
        logger.LogInformation("ApplyMigrationsAsync called");

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ICouchbaseMigrationContext>();

        context.Register(new Initial());

        await context.Apply();
    }
}

