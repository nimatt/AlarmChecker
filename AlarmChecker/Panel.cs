using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AlarmChecker
{
    public class Panel
    {
        [JsonProperty(PropertyName = "PanelId")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ArmedStatus")]
        public string ArmedStatus { get; set; }
    }
}
