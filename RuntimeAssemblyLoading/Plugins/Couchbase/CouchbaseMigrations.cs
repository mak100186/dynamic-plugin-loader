using CouchbasePlugin.Migrations;

using DnsClient.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

using Unibet.Infrastructure.Caching.CouchbaseV7.Migrations.Abstractions;

namespace CouchbasePlugin;

public class CouchbaseMigrations : IInjectedDependency, ICouchbaseMigrations
{
    private readonly ILogger<CouchbaseMigrations> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CouchbaseMigrations(ILogger<CouchbaseMigrations> logger, IServiceProvider serviceProvider)
    {
        this._logger = logger;
        this._serviceProvider = serviceProvider;    
    }

    public async Task ApplyMigrationsAsync()
    {
        _logger.LogInformation("ApplyMigrationsAsync called");

        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ICouchbaseMigrationContext>();

        context.Register(new Initial());

        await context.Apply();
    }
}

