using System;
using System.IO;
using EST.MIT.Payment.Function.Services;
using EST.MIT.Payment.Function.Validation;
using EST.MIT.Payment.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        if(hostContext.HostingEnvironment.IsDevelopment())
        {
            Console.WriteLine("STARTING IN DEVELOPMENT MODE");
            config.AddUserSecrets<Program>();
        }
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        Console.WriteLine("Program.Startup.ConfigureServices() called");
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        services.AddHttpClient();
        services.AddScoped<ISchemeValidator, SchemeValidator>();

        services.AddQueueAndServiceBusServices(configuration);
    })
    .Build();

host.Run();
