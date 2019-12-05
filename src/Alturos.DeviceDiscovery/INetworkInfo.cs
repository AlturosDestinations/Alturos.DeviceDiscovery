using System.Collections.Generic;

namespace Alturos.DeviceDiscovery
{
    public interface INetworkInfo
    {
        IList<NetworkInfo> GetAvailableNetworks();
    }
}
