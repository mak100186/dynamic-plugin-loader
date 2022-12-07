namespace CouchbasePlugin;

public interface ICouchbaseMigrations
{
    Task ApplyMigrationsAsync();
}

