using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static LibUSB.Functions.NativeMethods;
namespace LibUSB.Functions
{
	/// <summary>
	/// 
	/// </summary>
	public static class LibUSBNet
	{
		private static void LogForError(this string str) { }
		private static void LogForInfomation(this string str) { }
		private static void LogForDebug(this string str) { }

		private static IntPtr Context = IntPtr.Zero;
		/// <summary>
		/// 
		/// </summary>
		static LibUSBNet()
		{
			try
			{
				var iret = libusb_init(out Context);
				if (iret != 0)
				{
					Console.WriteLine($"Libusb Init Fail!{libusb_error_name(iret)}");
					return;
				}
				//libusb_set_debug(Context, 4);
			}
			catch
			{
				Console.WriteLine("初始化Libusb错误,请安装libusb!");
			}
		}

		/// <summary>
		/// 释放(预留)
		/// </summary>
		static void Exit()
		{
			libusb_exit(Context);
		}

		/// <summary>
		/// 根据VID PID在设备列表里面查找指定的设备,并返回和设备描述符index
		/// </summary>
		/// <param name="devs"></param>
		/// <param name="vid"></param>
		/// <param name="pid"></param>
		/// <param name="desc"></param>
		/// <returns>大于等于0:设备index -1:没找到</returns>
		public static int FindDevID(IntPtr[] devs, ushort vid, ushort pid, out LibUSB.Structures.Native.libusb_device_descriptor desc)
		{
			desc = default(LibUSB.Structures.Native.libusb_device_descriptor);
			for (int i = 0; i < devs.Length; i++)
			{
				var dev = devs[i];
				var iret = libusb_get_device_descriptor(dev, out desc);
				if (iret != 0) continue;
				if (desc.idVendor != vid || desc.idProduct != pid) continue;
				return i;
			}
			return -1;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="retdevices"></param>
		/// <returns></returns>
		public static int GetAllDevices(out ushort[][] retdevices)
		{
			var devices = new List<ushort[]>();
			retdevices = null;
			var iret = libusb_get_device_list(Context, out var plist);
			if (iret < 0)
			{
				$"libusb_get_device_list Fail!无法获取USB设备列表!iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
				return -1;
			}
			var devs = new IntPtr[iret];
			Marshal.Copy(plist, devs, 0, iret);
			var desc = default(LibUSB.Structures.Native.libusb_device_descriptor);
			for (int i = 0; i < devs.Length; i++)
			{
				var dev = devs[i];
				var iret2 = libusb_get_device_descriptor(dev, out desc);
				if (iret2 != 0) continue;
				devices.Add(new ushort[] { desc.idVendor, desc.idProduct });
			}
			libusb_free_device_list(plist, 1);
			retdevices = devices.ToArray();
			return 0;
		}

		/// <summary>
		/// 通过VID PID打开USB设备,并返回IO端口号
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="vid"></param>
		/// <param name="pid"></param>
		/// <param name="deviceinfo"></param>
		/// <param name="interfacenum"></param>
		/// <returns>0:成功</returns>
		public static int OpenByVidPid(ushort vid, ushort pid, out LibUSB.Structures.Net.DeviceInfo deviceinfo, int interfacenum = 0)
		{
			$"OpenUSB VID:{vid.ToString()}(0x{vid.ToString("X")}) PID:{pid.ToString()}(0x{pid.ToString("X")})".LogForInfomation();
			IntPtr dev = IntPtr.Zero;
			deviceinfo = new Structures.Net.DeviceInfo() { VID = vid, PID = pid };
			#region 枚举设备列表并根据VID PID找到目标设备
			var iret = libusb_get_device_list(Context, out var plist);
			if (iret < 0)
			{
				$"libusb_get_device_list Fail!无法获取USB设备列表!iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
				return -1;
			}
			var devlist = new IntPtr[iret];
			Marshal.Copy(plist, devlist, 0, iret);
			var devid = FindDevID(devlist, vid, pid, out var desc);
			if (devid < 0)
			{
				libusb_free_device_list(plist, 1);
				$"FindDev Fail!USB设备列表里找不到目标设备!iRet:{devid} Error:{libusb_error_name(devid)}".LogForError();
				return -1;
			}
			dev = devlist[devid];
			#endregion
			#region 枚举所有接口找到端点
			for (byte j = 0; j < desc.bNumConfigurations; j++)
			{
				libusb_get_config_descriptor(dev, j, out var pconf);
				var confdesc = new LibUSB.Structures.Native.libusb_config_descriptor();
				Marshal.PtrToStructure(pconf, confdesc);
				var E = confdesc.Linterface[0].Altsetting[0].EndPoint;
				foreach (var usbinterface in confdesc.Linterface) foreach (var altsetting in usbinterface.Altsetting) foreach (var endpoint in altsetting.EndPoint)
						{
							if ((endpoint.bEndpointAddress & 0x80) == 0)
							{
								deviceinfo.EndPoint_Out = endpoint.bEndpointAddress;
							}
							else
							{
								deviceinfo.EndPoint_In = endpoint.bEndpointAddress;
							}
							deviceinfo.InterfaceNumber = altsetting.bInterfaceNumber;//疑惑???
						}
				libusb_free_config_descriptor(pconf);
			}
			$"In:{deviceinfo.EndPoint_In} Out:{deviceinfo.EndPoint_Out}".LogForDebug();
			#endregion

			iret = libusb_open(dev, out deviceinfo.Handle);
			libusb_free_device_list(plist, 1);
			if (iret != 0)
			{
				$"libusb_open打开USB设备失败!iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
				return iret;
			}

			#region 替换驱动
			var isdetached = false;
			iret = libusb_kernel_driver_active(deviceinfo.Handle, 0);
			if (iret == 1) //?
			{
				iret = libusb_detach_kernel_driver(deviceinfo.Handle, 0);
				if (iret < 0)
				{
					$"libusb_detach_kernel_driver fail!iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
					libusb_close(deviceinfo.Handle);
					return iret;
				}
				isdetached = true;
			}
			iret = libusb_claim_interface(deviceinfo.Handle, interfacenum);
			$"libusb_claim_interface iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
			if (iret < 0)
			{
				iret = libusb_detach_kernel_driver(deviceinfo.Handle, 0);
				if (iret < 0)
				{
					$"libusb_detach_kernel_driver fail2!iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
					libusb_close(deviceinfo.Handle);
					return iret;
				}
				isdetached = true;
				iret = libusb_claim_interface(deviceinfo.Handle, interfacenum);
				if (iret < 0)
				{
					$"libusb_claim_interface fail!iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
					libusb_close(deviceinfo.Handle);
					return iret;
				}
			}

			if (isdetached)
			{
				/* 这一步可能需要在关闭设备的时候做? */
				//iret = libusb_attach_kernel_driver(deviceinfo.Handle, 0);
				/*
				if (iret < 0)
				{
					$"libusb_attach_kernel_driver fail!iRet:{iret} Error:{libusb_error_name(iret)}".LogForError();
					libusb_close(deviceinfo.Handle);
					return iret;
				}
				// */
			}
			$"OpenUSB Sucessfully!".LogForInfomation();
			#endregion
			return 0;
		}

	}//End Class
}