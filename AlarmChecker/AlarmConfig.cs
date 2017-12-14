using System;
using System.Collections.Generic;
using System.Text;

namespace AlarmChecker
{
    public class AlarmConfig
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string LoginPath { get; set; }
        public string StatusPath { get; set; }
    }
}
