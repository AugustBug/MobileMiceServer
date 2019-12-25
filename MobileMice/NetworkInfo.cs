using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MobileMice
{
    class NetworkInfo
    {
        public bool getLocalIp(out string localIp)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIp = ip.ToString();
                        return true;
                    }
                }
            }
            localIp = "";
            return false;
        }
    }
}
