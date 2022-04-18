using LoggingService.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace LoggingService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _watcher;
        private readonly string _inputFolder;
        private readonly string _logFileName;
        private readonly string _slackHookUrl;

        public Worker(ILogger<Worker> logger, IOptions<WatcherConfiguration> watcherOption)
        {
            _logger = logger;
            _inputFolder = watcherOption.Value.FolderDirectory;
            _logFileName = watcherOption.Value.LogFileName;
            _slackHookUrl = watcherOption.Value.SlackHookUrl;
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
                var client = new HttpClient();
                var jsonRequest = JsonConvert.SerializeObject(new { text = "Text File has been changed" });

                client.BaseAddress = new Uri(_slackHookUrl);
                client.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json") );

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress.AbsoluteUri);

                request.Content =  new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                _ = client.SendAsync(request)
                    .ContinueWith(responseTask =>
                    {
                        _logger.LogInformation($"Log Change at: {DateTime.Now}");
                    });
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