using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Frida.NetStandard;
using Microsoft.Ajax.Utilities;
using Lib.Utils.String;
using Lib.ADB;

namespace Lib.Frida
{
	public class FridaUtils : Application
	{
		private static readonly AdbUtils Adb;
		private static readonly DeviceManager DeviceManager;
		private static DeviceInfo adbDevice;
		public static Device fridaDevice;
		private readonly string CHECK_SERVER = "ps | grep frida-server";
		private readonly string CHECK_SUDO = "sudo";
		private readonly string LIST_SERVER = "ls /data/local/tmp/frida-server";
		private readonly string LOCAL_FILE = "frida-server";
		private readonly string REMOTE_FILE = "/data/local/tmp/frida-server";
		private readonly string RUN_SERVER = "sudo nohup ./data/local/tmp/frida-server < /dev/null &";
		private readonly string SUDO_LOCAL = "installsudo.sh";
		private readonly string SUDO_REMOTE = "/data/local/tmp/installsudo.sh";

		public ScriptWithRpc<IRpc> fridaScript;
		private string fridaServerId;
		private FridaSession fridaSession;

		static FridaUtils()
		{
			Adb = new AdbUtils();
			DeviceManager = new DeviceManager();
		}

		public FridaUtils(string deviceTypeName = "usb", string deviceId = "00c760ea528d3be8",
			string deviceName = "Nexus 5X")
		{
			var validType = Enum.TryParse(deviceTypeName, true, out FridaDeviceType deviceType);
			if (!validType)
				throw new NotSupportedException(
					$"Do not support type {deviceTypeName}, please use one of 'Local', 'Remote' and 'Usb'.");
			Type = deviceType;
			Name = deviceName;
			ID = deviceId;

			InitFridaServer();
		}

		private string Name { get; }
		private string ID { get; }

		private FridaDeviceType Type { get; }

		private void InitFridaServer()
		{
			if (adbDevice != null) Adb.Restart();

			adbDevice = Adb.ConnectToDevice(id: ID);

			var info = adbDevice.SendCommand(CHECK_SERVER);
			if (info.IsNullOrWhiteSpace())
			{
				info = adbDevice.SendCommand(LIST_SERVER).Trim();
				if (!info.Contains(REMOTE_FILE))
				{
					adbDevice.UploadFile(REMOTE_FILE, LOCAL_FILE);
					info = adbDevice.SendCommand($"su -c chmod 777 {REMOTE_FILE}");
					if (info.IsNotNullAndEmpty()) throw new ServerFailException("Cant set up frida server");
				}
			}

			StartServer();
		}

		public void StartApp(string packageName, string activityName)
		{
			adbDevice.StartApp(packageName, activityName);
		}

		public void AttachProcess(string processName)
		{
			var process = fridaDevice.EnumerateProcesses().FirstOrDefault(p => p.Name.Equals(processName));
			if (process == null)
				throw new NoProcessException($"Cant attach to process {processName}, may be need start app first.");

			try
			{
				fridaSession = fridaDevice.Attach(process.Pid);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public ScriptWithRpc<T> CreateScriptWithRpc<T>(string script)
		{
			return fridaSession.CreateScriptWithRpc<T>(script);
		}

		public void InjectScript(string script, Script.MessageDelegate e = null)
		{
			fridaScript?.Unload();
			fridaScript = fridaSession.CreateScriptWithRpc<IRpc>(script);
			fridaScript.OnMessage += (type, msg, data) => Console.WriteLine("Received message: " + msg);
			fridaScript.OnConsole += (level, msg) => Console.WriteLine($"[frida {level}] {msg}");
			fridaScript.Load();
		}

		private void StartServer()
		{
			var info = adbDevice.SendCommand(CHECK_SUDO);

			if (info.Contains("sudo: not found"))
			{
				adbDevice.UploadFile(SUDO_REMOTE, SUDO_LOCAL);
				adbDevice.SendCommand($"su -c chmod 777 {SUDO_REMOTE}");
				adbDevice.SendCommand($"sh {SUDO_REMOTE}");
			}

			adbDevice.SendCommand(RUN_SERVER);
			info = adbDevice.SendCommand(CHECK_SERVER);
			var arr = info.Trim().Split("\r\n");
			info = arr[arr.Length - 1];
			fridaServerId = info.Split("root")[1].Trim().Split(" ")[0];
			fridaDevice = DeviceManager.EnumerateDevices()
				.FirstOrDefault(d => d.Type.Equals(Type) && d.Id.Equals(ID));
			if (fridaDevice == null) throw new NoServerException("Cant connect frida server");
		}

		public void RestartServer()
		{
			adbDevice.SendCommand($"kill {fridaServerId}");
			StartServer();
		}
	}

	public interface IRpc
	{
		Msg pingScript(Input input);

		Task callBack();
	}

	public class Msg
	{
		public string message { get; set; }
		public int number { get; set; }
	}

	public class Input
	{
		public string author { get; set; }
	}
}