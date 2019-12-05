using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Alturos.DeviceDiscovery
{
    public class DefaultNetworkInfo : INetworkInfo
    {
        /// <summary>
        /// Get a list of networks including the IP address and subnet mask
        /// </summary>
        /// <returns></returns>
        public IList<NetworkInfo> GetAvailableNetworks()
        {
            return this.GetAllLocalNetworks(NetworkInterfaceType.Ethernet);
        }

        private NetworkInfo[] GetAllLocalNetworks(NetworkInterfaceType networkInterfaceType)
        {
            var networkInfos = new List<NetworkInfo>();
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == networkInterfaceType && networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            networkInfos.Add(new NetworkInfo
                            {
                                IPAddress = ip.Address.ToString(),
                                SubnetMask = ip.IPv4Mask.ToString()
                            });
                        }
                    }
                }
            }
            return networkInfos.ToArray();
        }
    }
}
