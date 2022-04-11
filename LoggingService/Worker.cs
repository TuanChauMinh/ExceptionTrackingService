using LoggingService.Configuration;
using Microsoft.Extensions.Options;

namespace LoggingService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _watcher;
        private readonly string _inputFolder;
        private readonly string _logFileName;

        public Worker(ILogger<Worker> logger, IOptions<WatcherConfiguration> watcherOption)
        {
            _logger = logger;
            _inputFolder = watcherOption.Value.FolderDirectory;
            _logFileName = watcherOption.Value.LogFileName;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tracking Service starting..........");

            if (!Directory.Exists(_inputFolder))
            {
                _logger.LogInformation($"Folder {_inputFolder} does not exist");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Set up event for Folder {_inputFolder}");
                           using var watcher = new FileSystemWatcher(_inputFolder);
            _watcher = new FileSystemWatcher(_inputFolder,"log.txt");
            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = true;
            _watcher.Changed += _watcher_Changed;

            return base.StartAsync(cancellationToken);


        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == $"{_inputFolder}\\{_logFileName}")
            {
                _logger.LogInformation($"Log Change at: {DateTime.Now}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;

        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");
            _watcher.EnableRaisingEvents = false;
            return base.StopAsync(cancellationToken);
        }



        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            
                _logger.LogInformation($"Log Change at: {DateTime.Now}");

        }
    }
}