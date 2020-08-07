using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using SharpAdbClient;

namespace Lib.ADB
{
	public class AdbUtils
	{
		private static readonly AdbServer Server = new AdbServer();
		private static readonly AdbClient Client = new AdbClient();
		private static DeviceData _device;

		public AdbUtils(string adbPath = "adb.exe", bool restartIfNewer = false)
		{
			Server.StartServer(adbPath, restartIfNewer);
		}

		private DeviceData GetDeviceById(string deviceId)
		{
			return Client.GetDevices().Find(d => d.Serial == deviceId);
		}

		public DeviceInfo ConnectToDevice(string host = "127.0.0.1", int port = 5037, string id = "")
		{
			_device = GetDeviceById(id);
			if (_device != null) return new DeviceInfo(_device ?? null, new ConsoleOutputReceiver());
			Client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
			_device = Client.GetDevices().FirstOrDefault();
			if (_device == null)
			{
				Restart();
				return ConnectToDevice(host, port, id);
			}

			return new DeviceInfo(_device, new ConsoleOutputReceiver());
		}

		public void Restart()
		{
			Server.RestartServer();
		}
	}

	public class DeviceInfo
	{
		private static readonly AdbClient Client = new AdbClient();
		private readonly DeviceData deviceData;
		private readonly IShellOutputReceiver receiver;

		public DeviceInfo(DeviceData deviceData, IShellOutputReceiver receiver = null)
		{
			this.deviceData = deviceData;
			this.receiver = receiver;
		}

		public string SetGeolocation(double latitude = 39.56, double longitude = 116.20)
		{
			Client.ExecuteRemoteCommand($"setprop persist.nox.gps.latitude {latitude}", deviceData, receiver);
			Client.ExecuteRemoteCommand($"setprop persist.nox.gps.longitude {longitude}", deviceData, receiver);
			return receiver.ToString();
		}

		public string StartApp(string packageName, string activityName)
		{
			Client.ExecuteRemoteCommand($"am start -n {packageName}/{activityName}", deviceData, receiver);
			return receiver.ToString();
		}

		public string StopApp(string packageName)
		{
			Client.ExecuteRemoteCommand($"am force-stop {packageName}", deviceData, receiver);
			return receiver.ToString();
		}

		public string Swipe(int x1, int y1, int x2, int y2)
		{
			Client.ExecuteRemoteCommand($"input swipe {x1} {y1} {x2} {y2}", deviceData, receiver);
			return receiver.ToString();
		}

		public string Tap(int x, int y)
		{
			Client.ExecuteRemoteCommand($"input tap {x} {y}", deviceData, receiver);
			return receiver.ToString();
		}

		public void DownloadFile(string remoteFile, string localFile)
		{
			using (var service =
				new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), deviceData))
			using (var stream = File.OpenWrite(localFile))
			{
				service.Pull(remoteFile, stream, null, CancellationToken.None);
			}
		}

		public void UploadFile(string remoteFile, string localFile)
		{
			using (var service =
				new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), deviceData))
			using (var stream = File.OpenRead(localFile))
			{
				service.Push(stream, remoteFile, 444, DateTime.Now, null, CancellationToken.None);
			}
		}

		public string SendCommand(string command)
		{
			receiver.Flush();
			Client.ExecuteRemoteCommand(command, deviceData, receiver);
			return receiver.ToString();
		}
	}
}