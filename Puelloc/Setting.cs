﻿using System.Net;

namespace Puelloc
{
    public class Setting
    {
        public void SetBindIP(string ip, int port)
        {
            BindIP = new IPEndPoint(IPAddress.Parse(ip), port);
        }
        public IPEndPoint BindIP { get; set; } = new IPEndPoint(IPAddress.Loopback, 80);
        /// <summary>
        /// If BindIP.Port is 0, this value will be ignored.
        /// </summary>
        public bool IsAutoChangePortIfHasUsed { get; set; } = true;
        public int BackLog { get; set; } = 50;
        public int ReceiveTimeOut { get; set; } = 10000;
        public int SendTimeOut { get; set; } = 10000;
        public string BasePath { get; set; }
        public string DefaultPage { get; set; } = "index.html";
    }
}
