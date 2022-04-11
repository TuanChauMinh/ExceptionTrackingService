using LoggingService;
using LoggingService.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddEventLog())
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.Configure<WatcherConfiguration>(configuration.GetSection(nameof(WatcherConfiguration)));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
