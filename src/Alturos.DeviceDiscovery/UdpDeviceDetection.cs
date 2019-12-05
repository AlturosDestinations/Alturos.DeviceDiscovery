using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Alturos.DeviceDiscovery
{
    public class UdpDeviceDetection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UdpDeviceDetection));

        public event Action<DeviceInfoPackage> DeviceResponseReceived;
        private readonly INetworkInfo _networkInfo;

        public UdpDeviceDetection(INetworkInfo networkInfo = null)
        {
            if (networkInfo == null)
            {
                this._networkInfo = new DefaultNetworkInfo();
                return;
            }

            this._networkInfo = networkInfo;
        }

        /// <summary>
        /// Send a broadcast package to the network and wait for an answer
        /// </summary>
        /// <param name="deviceDiscoverPort">The port on the device where discovery packets are answered.</param>
        /// <param name="deviceHelloPackage">The hello bytes where the device expects</param>
        /// <param name="timeout">The wait time for an answer after the hello package is send</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ScanAsync(int deviceDiscoverPort, byte[] deviceHelloPackage, int timeout = 1000, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tasks = new List<Task>();

            var networks = this._networkInfo.GetAvailableNetworks();
            foreach (var network in networks)
            {
                var task = Task.Run(async () =>
                {
                    var localEndpoint = new IPEndPoint(IPAddress.Parse(network.IPAddress), 0);

                    Log.Debug($"{nameof(ScanAsync)} - Start broadcast {network.IPAddress}");
                    using (var udpClient = new UdpClient(localEndpoint))
                    {
                        //The sendPort is the same on the device send the answer back
                        var sendPort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;

                        void dataReveived(byte[] data, IPAddress ipAddress)
                        {
                            var package = new DeviceInfoPackage
                            {
                                DiscoveredNetwork = network,
                                DeviceIpAddress = ipAddress.ToString(),
                                ReceivedData = data
                            };

                            this.DeviceResponseReceived?.Invoke(package);
                        }

                        using (var receiver = new UdpReceiver(sendPort))
                        {
                            receiver.DataReceived += dataReveived;
                            _= Task.Run(async () => await receiver.ReceiveAsync().ConfigureAwait(false));
                            await this.SendAsync(udpClient, deviceDiscoverPort, deviceHelloPackage).ConfigureAwait(false);
                            udpClient.Close();
                            await Task.Delay(timeout, cancellationToken).ConfigureAwait(false);
                            receiver.DataReceived -= dataReveived;
                        }
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a broadcast package to the network and wait for an answer
        /// </summary>
        /// <param name="deviceDiscoverPort">The port on the device where discovery packets are answered.</param>
        /// <param name="deviceHelloPackage">The hello bytes where the device expects</param>
        /// <param name="timeout">The wait time for an answer after the hello package is send</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DeviceInfoPackage[]> GetDeviceInfoPackagesAsync(int deviceDiscoverPort, byte[] deviceHelloPackage, int timeout = 1000, CancellationToken cancellationToken = default(CancellationToken))
        {
            var items = new List<DeviceInfoPackage>();

            void dataReveived(DeviceInfoPackage package)
            {
                items.Add(package);
            }

            this.DeviceResponseReceived += dataReveived;
            await this.ScanAsync(deviceDiscoverPort, deviceHelloPackage, timeout, cancellationToken).ConfigureAwait(false);
            this.DeviceResponseReceived -= dataReveived;

            return items.ToArray();
        }

        private async Task SendAsync(UdpClient udpClient, int destinationPort, byte[] deviceHelloPackage)
        {
            Log.Debug($"{nameof(SendAsync)} - Send hello package");

            var ipEndpoint = new IPEndPoint(IPAddress.Broadcast, destinationPort);
            await udpClient.SendAsync(deviceHelloPackage, deviceHelloPackage.Length, ipEndpoint).ConfigureAwait(false);
        }
    }
}
