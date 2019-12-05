# Alturos.DeviceDiscovery
Discover devices in a network over an udp broadcast

## NuGet
Quick install Alturos.DeviceDiscovery over [NuGet](https://www.nuget.org/packages/Alturos.DeviceDiscovery)
```
PM> install-package Alturos.DeviceDiscovery
```

## Example MOXA NPort discovery

```cs
var detection = new UdpDeviceDetection();
var helloPackage = new byte[] { 0x01, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };
var packages = await detection.GetDeviceInfoPackagesAsync(4800, helloPackage, timeout: 1000);
```
