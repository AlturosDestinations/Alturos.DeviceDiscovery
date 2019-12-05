namespace Alturos.DeviceDiscovery
{
    public class DeviceInfoPackage
    {
        public NetworkInfo DiscoveredNetwork { get; set; }
        public string DeviceIpAddress { get; set; }
        public byte[] ReceivedData { get; set; }
    }
}
