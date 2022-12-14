using RuntimeAssemblyLoading;

using Unibet.Infrastructure.Hosting.WebApi.Health;


//.net 6
var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews();

var shouldRunMigrationPathway = args.Contains("--migrate");
builder.Services.ConfigureServices(builder.Configuration, mvcBuilder, shouldRunMigrationPathway);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks(
        "/api/health",
        new() { ResponseWriter = HealthResponseFormatter.WriteResponse });
});

app.Run();

//todo POC:
//-change this to web host instead of generic host
//implement configurable plugin pipeline
//-register some services here (kafka, couchbase, postgres) and then let the plugin register some stuff
//add dbcontext here, pass down to plugin for use
//plugin should be able to add a new dbcontext and work with both
//-setup migration pathway
//-private repo to put this code base


/*
 * prototype should contain:
 * 1. plugin that brings freebet strategy
 * 2. plugin that brings uniboost strategy
 * 3. plugin that does service registrations with kafka, postgres, couchbase
 * 4. plugin that uses the configurable pipeline developed in the POC
 * 5. plugin that should be able to add its own migrations
 * 6. combine startup and program using .net 6
*/