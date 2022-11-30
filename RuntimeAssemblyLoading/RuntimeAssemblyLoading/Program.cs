using System.Reflection;
using System.Security.Cryptography;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RuntimeAssemblyLoading;

//await Host.CreateDefaultBuilder(args)
//    .ConfigureHostConfiguration(hostConfig =>
//    {
//        hostConfig.SetBasePath(Directory.GetCurrentDirectory());
//        hostConfig.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
//        hostConfig.AddEnvironmentVariables();
//        hostConfig.AddCommandLine(args);
//    })
//    .ConfigureServices(services =>
//    {
//        services.AddHostedService<HostApplication>();
//    })
//    .Build()
//    .RunAsync();

var options = new List<string> { "A", "B" };
Console.WriteLine("Demonstration of plugin loading at runtime");

while (true)
{
    Console.WriteLine("Select one from the following options: ");
    foreach (var option in options)
    {
        Console.WriteLine(option);
    }

    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || !options.Contains(input))
    {
        Console.WriteLine("Unknown option selected. Please try again.");
        continue;
    }
    
    var pluginName = $"RewardPlugin{input}";
    var assemblyName = $"{pluginName}.dll";
    var typeName = $"{pluginName}.Freebet";
    var propertyName = "Name";
    var methodName = "GetRn";
    var invocationCounterMethodName = "GetInvokeCounter";

    try
    {
        //can be appended with /France if depending on the actual implementation
        var currentAssembly = Assembly.GetCallingAssembly();
        var pathToCurrentAssembly = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty); 
        var pluginLoader = new PluginContext(pathToCurrentAssembly, assemblyName, typeName);

        Console.WriteLine($"GetName : {pluginLoader.InvokeProperty<string>(propertyName)}");

        var invokeAttempts = 1 + RandomNumberGenerator.GetInt32(9);
        Console.WriteLine($"GetRn() will be invoked {invokeAttempts} times");

        for (int i = 0; i < invokeAttempts; i++)
        {
            Console.WriteLine(pluginLoader.InvokeMethod<Guid>(methodName));
        }

        Console.WriteLine($"Invocation counter is {pluginLoader.InvokeMethod<int>(invocationCounterMethodName)}");

        Console.WriteLine($"Preferred way of calling is pluginLoader.GetInstance().GetRn(); and generates {pluginLoader?.GetInstance()?.GetRn()}");


    }    
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        continue;
    }
}

