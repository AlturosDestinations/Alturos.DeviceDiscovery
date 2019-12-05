using log4net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Alturos.DeviceDiscovery
{
    public class UdpReceiver : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UdpReceiver));

        public event Action<byte[], IPAddress> DataReceived;
        private readonly UdpClient _udpClient;
        private bool _disposed;

        public UdpReceiver(int port)
        {
            this._udpClient = new UdpClient(port);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._disposed = true;
            this._udpClient?.Close();
            this._udpClient?.Dispose();
        }

        public async Task ReceiveAsync()
        {
            try
            {
                while (!this._disposed)
                {
                    var result = await this._udpClient.ReceiveAsync().ConfigureAwait(false);
                    this.DataReceived?.Invoke(result.Buffer, result.RemoteEndPoint.Address);
                }
            }
            catch (ObjectDisposedException)
            {
                //No Log required
            }
            catch (Exception exception)
            {
                Log.Error(nameof(ReceiveAsync), exception);
            }
        }
    }
}
