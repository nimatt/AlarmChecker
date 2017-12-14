using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;


namespace AlarmChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddJsonFile("config-dev.json", true)
                .Build();

            var alarmConfig = configRoot.GetSection("alarm").Get<AlarmConfig>();
            var openHabConfig = configRoot.GetSection("openhab").Get<OpenHabConfig>();

            var openHab = new OpenHabService(openHabConfig);

            using (var service = new AlarmService(alarmConfig))
            {
                var currentStatus = AlarmStatus.None;
                while(true)
                {
                    var status = service.GetStatus().Result;
                    if (status != currentStatus)
                    {
                        Console.WriteLine("Alarm changed to " + status);
                        openHab.SetState(status != AlarmStatus.Disarmed).Wait();
                        currentStatus = status;
                    }
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
