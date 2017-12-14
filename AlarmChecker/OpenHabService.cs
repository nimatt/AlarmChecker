using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AlarmChecker
{
    class OpenHabService
    {
        private static HttpClient _client = new HttpClient();
        private OpenHabConfig Config { get; }

        public OpenHabService(OpenHabConfig config)
        {
            Config = config;
        }

        public async Task SetState(bool state) {
            var content = new StringContent(state ? "ON" : "OFF");
            var response = await _client.PutAsync($"http://{Config.Domain}/rest/items/{Config.Item}/state", content);
        }
    }
}
