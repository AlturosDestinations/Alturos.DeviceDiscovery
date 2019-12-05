using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace Alturos.DeviceDiscovery.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Start scan {DateTime.Now:HH:mm:ss,fff}");

            Console.WriteLine("########################## First test ################################");

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var detection = new UdpDeviceDetection();
            var packages = detection.GetDeviceInfoPackagesAsync(5555, new byte[] { 0x43, 0x30, 0x32 }, timeout: 1000).GetAwaiter().GetResult();
            foreach (var package in packages)
            {
                Console.WriteLine($"{package.DiscoveredNetwork.IPAddress} {package.DeviceIpAddress} {BitConverter.ToString(package.ReceivedData)}");
            }

            Console.WriteLine("########################## Second test ###############################");

            detection.DeviceResponseReceived += Detection_DeviceResponseReceived;
            detection.ScanAsync(5555, new byte[] { 0x43, 0x30, 0x32 }, timeout: 1000).GetAwaiter().GetResult();
            detection.DeviceResponseReceived -= Detection_DeviceResponseReceived;

            Console.WriteLine($"Scan done {DateTime.Now:HH:mm:ss,fff}");
            Console.ReadLine();
        }

        private static void Detection_DeviceResponseReceived(DeviceInfoPackage package)
        {
            Console.WriteLine($"{package.DiscoveredNetwork.IPAddress} {package.DeviceIpAddress} {BitConverter.ToString(package.ReceivedData)}");
        }
    }
}
