namespace MonsterWebShop.Models
{
    public class Log
    {
        public int LogID { get; set; }

        public string IPAddress { get; set; } = string.Empty;

        public DateTime WhatTime { get; set; }

        public string? WhatHappened { get; set; }

        public string? SessionID { get; set; }

        public string? IncidentType { get; set; }
    }
}