using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using AutoMapper;

using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;
using RuntimeAssemblyLoading.Helpers;
using RuntimeAssemblyLoading.Services;
using RuntimeAssemblyLoading.Services.Plugin;

using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates;
using Serilog.Templates.Themes;

using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

using Unibet.Infrastructure.Caching.CouchbaseV7;
using Unibet.Infrastructure.Hosting.WebApi.Configuration;
using Unibet.Infrastructure.Hosting.WebApi.Filters;
using Unibet.Infrastructure.Hosting.WebApi.Health;

namespace RuntimeAssemblyLoading;
public static class ServiceRegistrations
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config, bool shouldRunMigrationPathway)
    {
        //No DbContext, No Couchbase, No Kafka - they should be loaded by their respective plugins. 
        services.AddControllers(x => x.AllowEmptyInputInBodyModelBinding = true)
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        var builder = services.AddHealthChecks()
            .AddCheck<InstanceDiagnosticsCheck>(nameof(InstanceDiagnosticsCheck));

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.Configure<StartUpOptions>(options =>
        {
            options.ShouldRunMigrationPathway = shouldRunMigrationPathway;
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddMvc(c =>
        {
            c.Filters.ConfigureFilters();

            // https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnetcore#mvc-async-suffix-trimmed-from-controller-action-names
            c.SuppressAsyncSuffixInActionNames = false;
        })
        .AddNewtonsoftJson()
        .AddFluentValidation()
        .AddControllersAsServices();

        services.AddKspCouchbase7(config)
            .EnableMigrations(config);

        services.AddModelMappings();
        services.AddRestClientWrapper();
        services.AddSwaggerRegistrations();
        services.AddWebHostServices();
        services.AddHostedService<HostApplication>();

        services.LoadPluginDependencies(config);
    }

    public static void LoadPluginDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        PluginDependenciesLoader.LoadDependencies(services, configuration);
    }

    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((ctx, conf) =>
        {
            conf.ReadFrom.Configuration(ctx.Configuration);
            //conf.WriteTo.Console(new ExpressionTemplate("{ \n { \n @t, @mt, @l: if @l = 'Information' then undefined() else @l, @x, ..@p \n} \n},\n", theme: TemplateTheme.Code));  
            conf.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}{NewLine}");
            conf.WriteTo.File(path: "log-.txt", outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}{NewLine}", rollingInterval: RollingInterval.Day);
        });
    }

    public static void ConfigureFilters(this FilterCollection filterCollection)
    {
        var filters = new SortedList<int, Type>
        {
            { 10, typeof(DefaultExceptionHandlingFilter) },
            { 30, typeof(RequestAuditFilter) },
            { 40, typeof(ValidateModelStateFilter) }
        };

        foreach (var (key, value) in filters)
        {
            filterCollection.Add(value, key);
        }
    }

    public static void AddModelMappings(this IServiceCollection services)
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddModelMappings();
        });

        var mapper = config.CreateMapper();

        services.AddTransient(_ => mapper);
    }

    public static IMapper AddModelMappings(this IMapperConfigurationExpression cfg)
    {
        //todo: add profiles here e.g. cfg.AddProfile<ApiToDomainMapping>();

        var mapper = new MapperConfiguration((MapperConfigurationExpression)cfg).CreateMapper();
        return mapper;
    }

    public static IServiceCollection AddRestClientWrapper(this IServiceCollection services)
    {
        services.AddSingleton<Func<string, int, IRestClientWrapper>>((baseUrl, timeout) => new RestClientWrapper(baseUrl, timeout));
        services.AddSingleton<Func<string, IRestClientWrapper>>(baseUrl => new RestClientWrapper(baseUrl));

        return services;
    }
    public static void AddWebHostServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginMigrator, PluginMigrator>();
    }
    public static void AddSwaggerRegistrations(this IServiceCollection services)
    {
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, SwaggerGenVersionedConfiguration>();
        services.AddSwaggerGen(o =>
        {
            o.EnableAnnotations();
            o.ExampleFilters();
            o.OperationFilter<SwaggerDefaultValues>();
            o.OperationFilter<SwaggerCorrelationIdHeaderFilter>();
            o.OperationFilter<SwaggerIgnoreClassNameParameterFilter>();
        });
        //services.AddSwaggerExamplesFromAssemblyOf<Startup>();
        services.AddApiVersioning(config =>
        {
            config.ReportApiVersions = true;
            config.DefaultApiVersion = new(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;

            config.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader(),
                new HeaderApiVersionReader
                {
                    HeaderNames = { "x-api-version" }
                });
        });

        services.AddVersionedApiExplorer(
            options =>
            {
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";
            });
    }
}


public class SwaggerGenVersionedConfiguration : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;
    private readonly string title;
    private readonly string description;

    public SwaggerGenVersionedConfiguration(IApiVersionDescriptionProvider provider, IConfiguration config)
    {
        this.provider = provider;
        const string key = "OpenApiInfo:Title";
        this.title = config.GetValue<string>(key);
        this.description = config.GetValue<string>("OpenApiInfo:Description");
    }

    public void Configure(SwaggerGenOptions options)
    {
        // Filter out version parameters globally
        options.OperationFilter<ApiVersionFilter>();

        // Add api info
        foreach (var versionDescription in this.provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(versionDescription.GroupName, this.CreateInfoForApiVersion(versionDescription));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription apiVersionDescription)
    {
        var infoForApiVersion = new OpenApiInfo
        {
            Title = this.title,
            Version = apiVersionDescription.ApiVersion.ToString(),
            Description = this.description
        };

        if (apiVersionDescription.IsDeprecated)
        {
            infoForApiVersion.Description += " This API version has been deprecated.";
        }

        return infoForApiVersion;
    }

    internal class ApiVersionFilter : IOperationFilter
    {
        private readonly List<string> parameterNamesToRemove = new() { "api-version", "x-api-version" };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parametersToRemove = operation.Parameters.Where(x => this.parameterNamesToRemove.Contains(x.Name)).ToList();
            foreach (var parameter in parametersToRemove)
            {
                operation.Parameters.Remove(parameter);
            }
        }
    }
}

public class SwaggerIgnoreClassNameParameterFilter : IOperationFilter
{
    private const string Pattern = @"^.*?\.";

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters
            .Where(x => x.In == ParameterLocation.Query && x.Name.Contains(".")))
        {
            parameter.Name = Regex.Replace(
                parameter.Name,
                Pattern,
                string.Empty,
                RegexOptions.IgnorePatternWhitespace);
        }
    }
}