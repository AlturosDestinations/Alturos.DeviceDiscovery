using log4net;
using System;
using System.Net;
using System.Net.Sockets;

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

        public void StartListening()
        {
            try
            {
                if (this._disposed)
                {
                    return;
                }

                this._udpClient.BeginReceive(this.Receive, null);
            }
            catch (Exception exception)
            {
                Log.Error(nameof(StartListening), exception);
            }
        }

        private void Receive(IAsyncResult ar)
        {
            try
            {
                if (this._disposed)
                {
                    return;
                }

                var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var bytes = this._udpClient.EndReceive(ar, ref ipEndPoint);

                Log.Debug($"{nameof(Receive)} - {BitConverter.ToString(bytes)}");

                this.DataReceived?.Invoke(bytes, ipEndPoint.Address);

                this.StartListening();
            }
            catch (ObjectDisposedException)
            {
                //No Log required
            }
            catch (Exception exception)
            {
                Log.Error(nameof(Receive), exception);
            }
        }
    }
}
