using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AlarmChecker
{
    class AlarmService : IDisposable
    {
        private Regex CookieExp = new Regex("[.]ASPXAUTH=(?<auth>[^;]+).*(expires=(?<exp>[^;]+))?");
        private HttpClient client;
        private HttpClientHandler handler;
        private bool loggedIn;

        private AlarmConfig Config { get; }

        public AlarmService(AlarmConfig config)
        {
            handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.AllowAutoRedirect = false;
            client = new HttpClient(handler);
            Config = config;
        }

        private async Task<bool> Login()
        {
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("userID", Config.User),
                new KeyValuePair<string, string>("password", Config.Password)
            });

            using (var loginResponse = await client.PostAsync($"https://{Config.Domain}{Config.LoginPath}", content))
            {

                Cookie cookie = null;
                var cookieMatch = loginResponse.Headers.GetValues("Set-Cookie")
                    .Select(val => CookieExp.Match(val))
                    .FirstOrDefault(m => m.Success);

                if (cookieMatch != null)
                {
                    cookie = new Cookie(".ASPXAUTH", cookieMatch.Groups["auth"].Value, "/", Config.Domain);
                    handler.CookieContainer.Add(cookie);
                    return true;
                }
            }

            return false;
        }

        public async Task<AlarmStatus> GetStatus()
        {
            if (!loggedIn)
            {
                loggedIn = await Login();
                if (!loggedIn)
                {
                    return AlarmStatus.None;
                }
            }

            using (var response = await client.GetAsync($"https://{Config.Domain}{Config.StatusPath}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await ParseResponse(response);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    loggedIn = false;
                    return await GetStatus();
                }
            }
            return AlarmStatus.None;
        }

        private static async Task<AlarmStatus> ParseResponse(HttpResponseMessage response)
        {
            using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var panel = JsonSerializer.Create().Deserialize<Panel[]>(jsonReader).FirstOrDefault();
                if (panel != null)
                {
                    switch (panel.ArmedStatus)
                    {
                        case "disarmed":
                            return AlarmStatus.Disarmed;
                        case "partialarmed":
                            return AlarmStatus.Partial;
                        default:
                            break;
                    }
                }

                return AlarmStatus.None;
            }
        }

        public void Dispose()
        {
            client?.Dispose();
            handler?.Dispose();
        }
    }
}
