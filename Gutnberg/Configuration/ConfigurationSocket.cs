using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Project_Gutenberg.Configuration
{
    public record ConfigurationSocket : IConfiguration
    {        
        public ProtocolType protocolType = ProtocolType.Unknown;
        public int retrayConnection      = 10;
        public int retrayWait            = 1000;
        public int backlog               = 10;
        public int reciveBufferSize      = 1024;

        public IPEndPoint? ipEndPoint;
        public Socket? socket;

        public ConfigurationSocket SetEndPoint(string adress, int port)
        {
            IPAddress[] ipaddress = Dns.GetHostAddresses(adress);
            ipEndPoint = new IPEndPoint(ipaddress.First(), port);

            return this;
        }
        public ConfigurationSocket SetProtocolType(ProtocolType protocolType)
        {
            this.protocolType = protocolType;
            return this;
        }
    }
}
