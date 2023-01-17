using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PemberiMaklumatIBR900keMSSQL
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes("admin:cradlepoint2020");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var responseGps = await client.GetAsync("http://10.192.23.1/api/status/gps/fix");
                var contentGps = await responseGps.Content.ReadAsStringAsync();
                Console.WriteLine("Content GPS: " + contentGps);
                var responseGps = await client.GetAsync("http://10.192.23.1/api/status/stats/signal_history");
                var contentGps = await responseGps.Content.ReadAsStringAsync();
                Console.WriteLine("Content GPS: " + contentGps);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}