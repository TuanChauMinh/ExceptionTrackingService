using LoggingService;
using LoggingService.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddEventLog())
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.Configure<WatcherConfigurationEntity>(configuration.GetSection(nameof(WatcherConfigurationEntity)));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
