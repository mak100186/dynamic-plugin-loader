using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using RuntimeAssemblyLoading;

using Unibet.Infrastructure.Hosting.WebApi.Health;


//.net 6
var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureSerilog();

var shouldRunMigrationPathway = args.Contains("--migrate");
builder.Services.ConfigureServices(shouldRunMigrationPathway);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks(
        "/api/health",
        new() { ResponseWriter = HealthResponseFormatter.WriteResponse });
});

app.Run();


/*
//.net 5
var host = Host.CreateDefaultBuilder(args)
    .ConfigureSerilog()
    .ConfigureWebHost()
    .Build();

//using var scope = host.Services.CreateScope();
//var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

//await Migrations.ApplyAsync(async () =>
//{
//    logger.LogInformation("Applications migration code path invoked");

//    //todo: execute plugin migration code path here

//    //using var context = scope.ServiceProvider.GetRequiredService<IDbContextPlugin>();
//    //await context.DropAsync();
//    //context.Migrate();

//    //scope.ServiceProvider.SeedTemplates(); //todo: should go to plugin

//    //await using var context = scope.ServiceProvider.GetRequiredService<ICouchbaseMigration>();
//    //context.Register(new Initial());
//    //await context.Apply();

//    //start plugin loader with --migrate

//});

//logger.LogInformation("Applications execution code path invoked");

await host.RunAsync();
*/

//todo POC:
//change this to web host instead of generic host
//implement configurable plugin pipeline
//register some services here (kafka, couchbase, postgres) and then let the plugin register some stuff
//add dbcontext here, pass down to plugin for use
//plugin should be able to add a new dbcontext and work with both
//setup migration pathway
//private repo to put this code base


/*
 * prototype should contain:
 * 1. plugin that brings freebet strategy
 * 2. plugin that brings uniboost strategy
 * 3. plugin that does service registrations with kafka, postgres, couchbase
 * 4. plugin that uses the configurable pipeline developed in the POC
 * 5. plugin that should be able to add its own migrations
 * 6. combine startup and program using .net 6
*/