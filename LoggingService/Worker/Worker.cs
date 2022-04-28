using LoggingService.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LoggingService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _watcher;
        private readonly string _inputFolder;
        private readonly string _logFileName;
        private readonly string _slackHookUrl;

        //private readonly List<string> exceptionKeywords = new List<string> { "Exception", "Error" };

        public Worker(ILogger<Worker> logger, IOptions<WatcherConfigurationEntity> watcherOption)
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
                var data = new NameValueCollection();
                data["token"] = "";
                data["channel"] = "";
                data["as_user"] = "true";
               
                //data["attachments"] = "[{\"fallback\":\"dummy\", \"text\":\"this is an attachment\"}]";
                var client = new WebClient();
                var fileLines = File.ReadAllLines(e.FullPath).TakeLast(100).ToList();

                var index = 0;
                foreach (var line in fileLines)
                {
                    if(line.Contains("Error"))
                    {
                        dynamic jsonObject = new JObject();
                        jsonObject.text = line;
                        jsonObject.fallback = "dummy";

                        data["text"] = "EXCEPTION APPEARED \n" + line;
                        data["attachments"] = "[{\"fallback\":\"dummy\", \"text\":\""+ fileLines[index + 1] + "\"}]";

                        var response = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", data);
                    }

                    index++;
                }


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
    }
}