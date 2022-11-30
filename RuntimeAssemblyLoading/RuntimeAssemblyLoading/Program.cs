using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RuntimeAssemblyLoading.Services;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<HostApplication>();
    })
    .Build()
    .RunAsync();

//todo POC:
//change this to web host instead of generic host
//implement configurable plugin pipeline
//register some services here (kafka, couchbase, postgres) and then let the plugin register some stuff
//add dbcontext here, pass down to plugin for use
//plugin should be able to add a new dbcontext and work with both
//setup migration pathway


/*
 * prototype should contain:
 * 1. plugin that brings freebet strategy
 * 2. plugin that brings uniboost strategy
 * 3. plugin that does service registrations with kafka, postgres, couchbase
 * 4. plugin that uses the configurable pipeline developed in the POC
 * 5. plugin that should be able to add its own migrations
*/