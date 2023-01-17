using System;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                string connectionString = "Data Source=0.tcp.ap.ngrok.io,17010; Initial Catalog=tagvisnest; User Id=sa; Password=P@ssw0rd1; Integrated Security=False";
                string query = "SELECT ip_address FROM tagvisnest.dbo.view_deviceTable";
                string[] ipList;
                // Create a new SqlConnection and SqlCommand
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Open the connection and execute the query
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    // Create a list to hold the results
                    var data = new List<string>();

                    // Read the data and add it to the list
                    while (reader.Read())
                    {
                        data.Add(reader["ip_address"]?.ToString());
                    }
                    reader.Close();
                    connection.Close();
                    ipList = data.ToArray();
                }
                var byteArray = Encoding.ASCII.GetBytes("admin:cradlepoint2020");
                Parallel.ForEach(ipList, async IP => {
                    try
                    {
                        // Get request to get gps and signal info
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        var responseGps = await client.GetAsync("http://" + IP + "/api/status/gps/fix");
                        var contentGps = await responseGps.Content.ReadAsStringAsync();
                        var responseSignal = await client.GetAsync("http://" + IP + "/api/status/stats/signal_history");
                        var contentSignal = await responseGps.Content.ReadAsStringAsync();
                        Console.WriteLine($@"{Environment.NewLine} Pekerja mencari IP = {IP} {Environment.NewLine}  Content Signal: {Environment.NewLine} {contentSignal} {Environment.NewLine} Content GPS: {Environment.NewLine} {contentGps} {Environment.NewLine}");

                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine("\nException Caught!" + $@" Pekerja gagal dapatkan data pada IP = {IP}");
                        Console.WriteLine("Message :{0} ", e.Message);
                    }
                });
                
                /*
                // Simple sql query get data from table
                string connectionString = "Data Source=0.tcp.ap.ngrok.io,17010; Initial Catalog=tagvisnest; User Id=sa; Password=P@ssw0rd1; Integrated Security=False";
                string tableName = "tagvisnest.dbo.view_deviceTable"; //salah table 
                List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName}";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            var resultsTP = new List<Dictionary<string, object>>();
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(reader.GetName(i), reader[i]);
                                }
                                resultsTP.Add(row);
                            }
                            results = resultsTP;
                        }
                    }
                    connection.Close();
                }
                var jsonString = JsonSerializer.Serialize(results);
                Console.WriteLine(jsonString);
                */

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}