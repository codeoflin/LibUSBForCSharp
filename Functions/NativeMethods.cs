using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using LibUSB.Structures.Native;

namespace LibUSB.Functions
{
	using libusb_context = IntPtr;
	using libusb_device_array = IntPtr;
	using libusb_device = IntPtr;
	using libusb_device_handle = IntPtr;
	using libusb_config_descriptor = IntPtr;
	/// <summary>
	/// 
	/// </summary>
	public static class NativeMethods
	{
		[DllImport("kernel32")]
		private static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.LPStr)] string fileName);
		/// <summary>
		/// 
		/// </summary>
		public const string LIBUSB = "libusb-1.0";

		static NativeMethods()
		{
			var runpath = Path.GetDirectoryName(typeof(NativeMethods).Assembly.Location);
			var basepath = $"{runpath}/runtimes/win-{(Environment.Is64BitProcess ? "x64" : "x86")}/native/";//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
#if NETCOREAPP
			//为了实现x64和x86共兼容,此处必须手动加载dll
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))LoadLibraryA($"{basepath}/libusb-1.0.dll");
#else
			LoadLibraryA($"{basepath}/libusb-1.0.dll");
#endif
		}

		#region libusb_error_name
		/// <summary>
		/// Returns a constant NULL-terminated string with the ASCII name of a libusb error or transfer status code. The caller must not free() the returned string.
		/// </summary>
		/// <param name="error_code">The libusb_error or libusb_transfer_status code to return the name of.</param>
		/// <returns></returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "libusb_error_name")]
		private static extern IntPtr _libusb_error_name(int error_code);

		/// <summary>
		/// Returns a constant NULL-terminated string with the ASCII name of a libusb error or transfer status code. The caller must not free() the returned string.
		/// </summary>
		/// <param name="code">The libusb_error or libusb_transfer_status code to return the name of.</param>
		/// <returns></returns>
		public static string libusb_error_name(int code)
		{
			var pstr = _libusb_error_name(code);
			return Marshal.PtrToStringAnsi(pstr);
		}
		#endregion

		/// <summary>
		/// Returns a pointer to const struct libusb_version with the version (major, minor, micro, nano and rc) of the running library. 
		/// </summary>
		/// <returns>Returns a pointer to const struct libusb_version with the version (major, minor, micro, nano and rc) of the running library. </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "libusb_get_version")]
		private static extern IntPtr _libusb_get_version();

		/// <summary>
		/// Returns a pointer to const struct libusb_version with the version (major, minor, micro, nano and rc) of the running library. 
		/// </summary>
		/// <returns>Returns a pointer to const struct libusb_version with the version (major, minor, micro, nano and rc) of the running library. </returns>
		public static libusb_version libusb_get_version()
		{
			var ptr = _libusb_get_version();
			var obj = new libusb_version();
			obj = Marshal.PtrToStructure<libusb_version>(ptr);
			return obj;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="level">
		/// LIBUSB_LOG_LEVEL_NONE (0) : no messages ever printed by the library (default)
		/// LIBUSB_LOG_LEVEL_ERROR (1) : error messages are printed to stderr
		/// LIBUSB_LOG_LEVEL_WARNING (2) : warning and error messages are printed to stderr
		/// LIBUSB_LOG_LEVEL_INFO (3) : informational messages are printed to stderr
		/// LIBUSB_LOG_LEVEL_DEBUG (4) : debug and informational messages are printed to stderr
		/// </param>
		/// <returns></returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_set_debug(libusb_context context, int level);

		#region 
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int libusb_set_log_cb(libusb_context context, int level);

		private static void libusb_log_cb(IntPtr ctx, int level, IntPtr str)
		{

		}
		#endregion

		/// <summary>
		/// Initialize libusb. This function must be called before calling any other libusb function.
		///	If you do not provide an output location for a context pointer, a default context will be created. If there was already a default context, it will be reused (and nothing will be initialized/reinitialized).
		/// 初始化libusb,必须调用过这个,才能开始使用libusb.
		/// </summary>
		/// <param name="context">Optional output location for context pointer. Only valid on return code 0. 传入一个指针用于接收libusb上下文.只有libusb_init返回值为0的情况下此参数有效. </param>
		/// <returns>0 on success, or a LIBUSB_ERROR code on failure </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_init(out libusb_context context);

		/// <summary>
		/// Deinitialize libusb. Should be called after closing all open devices and before your application terminates. 
		/// </summary>
		/// <param name="context">the context to deinitialize, or NULL for the default context </param>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern void libusb_exit(libusb_context context);

		/// <summary>
		/// Returns a list of USB devices currently attached to the system. This is your entry point into finding a USB device to operate.
		/// You are expected to unreference all the devices when you are done with them, and then free the list with libusb_free_device_list(). Note that libusb_free_device_list() can unref all the devices for you. Be careful not to unreference a device you are about to open until after you have opened it.
		/// This return value of this function indicates the number of devices in the resultant list. The list is actually one element larger, as it is NULL-terminated.
		/// 返回当前系统挂载的USB设备列表
		/// 这个列表可以让你在其中寻找你要操作的USB设备
		/// 这个函数返回的列表需要通过libusb_free_device_list释放.释放列表会导致列表中的一些信息无效,请在打开设备后再释放列表!(已经打开的USB设备句柄与列表无关)
		/// 这个函数的返回值表示列表长度
		/// </summary>
		/// <param name="ctx">the context to operate on, or NULL for the default context libusb上下文</param>
		/// <param name="list">output location for a list of devices. Must be later freed with libusb_free_device_list(). </param>
		/// <returns>the number of devices in the outputted list, or any libusb_error according to errors encountered by the backend. </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "libusb_get_device_list")]
		public static extern int libusb_get_device_list(libusb_context ctx, out libusb_device_array list);

		/*
		public static int libusbnet_get_device_list(libusb_context ctx, out IntPtr[] list)
		{
			list = new IntPtr[0];
			var iret = libusb_get_device_list(ctx, out IntPtr _list);
			if (iret <= 0) return iret;
			list = new IntPtr[iret];
			Marshal.Copy(_list, list, 0, iret);
		}
		// */

		/// <summary>
		/// Frees a list of devices previously discovered using libusb_get_device_list(). If the unref_devices parameter is set, the reference count of each device in the list is decremented by 1.
		/// 释放libusb_get_device_list返回的列表
		/// 如果unref_devices传入了1,则列表中所有设备的引用计数-1
		/// </summary>
		/// <param name="list">the list to free 要释放的列表</param>
		/// <param name="unref_devices">whether to unref the devices in the list 是否取消对列表中设备的引用</param>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern void libusb_free_device_list(libusb_device_array list, int unref_devices);

		/// <summary>
		/// Get the USB device descriptor for a given device. 
		/// This is a non-blocking function; the device descriptor is cached in memory.
		/// Note since libusb-1.0.16, LIBUSB_API_VERSION >= 0x01000102, this function always succeeds.
		/// 获取指定USB设备的描述符
		/// 这是个非阻塞函数,所获取的内容是缓存.
		/// 自从 libusb-1.0.16, LIBUSB_API_VERSION >= 0x01000102 版本之后,这个函数必定调用成功.
		/// </summary>
		/// <param name="dev">the devices 设备</param>
		/// <param name="desc">output location for the descriptor data 接收返回描述符的变量</param>
		/// <returns>0 on success or a LIBUSB_ERROR code on failure 返回0表示成功,如果失败会返回一个LIBUSB_ERROR代码</returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_get_device_descriptor(libusb_device dev, out libusb_device_descriptor desc);

		/// <summary>
		/// Get a USB configuration descriptor based on its index. This is a non-blocking function which does not involve any requests being sent to the device.
		/// </summary>
		/// <param name="dev">a device</param>
		/// <param name="config_index">the index of the configuration you wish to retrieve</param>
		/// <param name="config">	output location for the USB configuration descriptor. Only valid if 0 was returned. Must be freed with libusb_free_config_descriptor() after use.</param>
		/// <returns></returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_get_config_descriptor(libusb_device dev, byte config_index, out libusb_config_descriptor config);

		/// <summary>
		/// Free a configuration descriptor obtained from libusb_get_active_config_descriptor() or libusb_get_config_descriptor(). It is safe to call this function with a NULL config parameter, in which case the function simply returns.
		/// </summary>
		/// <param name="config">config	the configuration descriptor to free</param>
		/// <returns></returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_free_config_descriptor(libusb_config_descriptor config);

		/// <summary>
		/// Open a device and obtain a device handle. A handle allows you to perform I/O on the device in question. 
		/// Internally, this function adds a reference to the device and makes it available to you through libusb_get_device(). 
		/// This reference is removed during libusb_close(). This is a non-blocking function; no requests are sent over the bus.
		/// </summary>
		/// <param name="dev">the dev to open 要打开的设备</param>
		/// <param name="dev_handle">output location for the returned device handle pointer. Only populated when the return code is 0.</param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NO_MEM on memory allocation failure 
		/// LIBUSB_ERROR_ACCESS if the user has insufficient permissions 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// another LIBUSB_ERROR code on other failure
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_open(libusb_device dev, out libusb_device_handle dev_handle);

		/// <summary>
		/// Convenience function for finding a device with a particular idVendor/idProduct combination. This function is intended for those scenarios where you are using libusb to knock up a quick test application - it allows you to avoid calling libusb_get_device_list() and worrying about traversing/freeing the list. 
		/// This function has limitations and is hence not intended for use in real applications: if multiple devices have the same IDs it will only give you the first one, etc.
		/// </summary>
		/// <param name="ctx">the context to operate on, or NULL for the default context</param>
		/// <param name="vendor_id">the idVendor value to search for</param>
		/// <param name="product_id">the idProduct value to search for</param>
		/// <returns>a device handle for the first found device, or NULL on error or if the device could not be found.</returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern libusb_device_handle libusb_open_device_with_vid_pid(libusb_context ctx, ushort vendor_id, ushort product_id);
		/// <summary>
		/// Close a device handle. Should be called on all open handles before your application exits.
		/// Internally, this function destroys the reference that was added by libusb_open() on the given device.
		/// This is a non-blocking function; no requests are sent over the bus.
		/// </summary>
		/// <param name="dev_handle">a device handle for the first found device, or NULL on error or if the device could not be found.</param>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern void libusb_close(libusb_device_handle dev_handle);

		/// <summary>
		/// Get the underlying device for a device handle. This function does not modify the reference count of the returned device, so do not feel compelled to unreference it when you are done. 
		/// 获取设备句柄的基础设备。此函数不会修改返回设备的引用计数，因此当您完成此操作时，不要觉得有必要取消引用它。
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <returns>the underlying device</returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern libusb_device libusb_get_device(libusb_device_handle dev_handle);

		/// <summary>
		/// Determine the bConfigurationValue of the currently active configuration.
		/// You could formulate your own control request to obtain this information, but this function has the advantage that it may be able to retrieve the information from operating system caches (no I/O involved).
		/// If the OS does not cache this information, then this function will block while a control transfer is submitted to retrieve the information.
		/// This function will return a value of 0 in the config output parameter if the device is in unconfigured state.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="config">output location for the bConfigurationValue of the active configuration (only valid for return code 0)</param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// another LIBUSB_ERROR code on other failure
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_get_configuration(libusb_device_handle dev_handle, ref int config);

		/// <summary>
		/// Set the active configuration for a device.
		/// The operating system may or may not have already set an active configuration on the device. It is up to your application to ensure the correct configuration is selected before you attempt to claim interfaces and perform other operations.
		/// If you call this function on a device already configured with the selected configuration, then this function will act as a lightweight device reset: it will issue a SET_CONFIGURATION request using the current configuration, causing most USB-related device state to be reset (altsetting reset to zero, endpoint halts cleared, toggles reset).
		/// You cannot change/reset configuration if your application has claimed interfaces. It is advised to set the desired configuration before claiming interfaces.
		/// Alternatively you can call libusb_release_interface() first. Note if you do things this way you must ensure that auto_detach_kernel_driver for dev is 0, otherwise the kernel driver will be re-attached when you release the interface(s).
		/// You cannot change/reset configuration if other applications or drivers have claimed interfaces.
		/// A configuration value of -1 will put the device in unconfigured state. The USB specifications state that a configuration value of 0 does this, however buggy devices exist which actually have a configuration 0.
		/// You should always use this function rather than formulating your own SET_CONFIGURATION control request. This is because the underlying operating system needs to know when such changes happen.
		/// This is a blocking function.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="configuration">the bConfigurationValue of the configuration you wish to activate, or -1 if you wish to put the device in an unconfigured state </param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NOT_FOUND if the requested configuration does not exist 
		/// LIBUSB_ERROR_BUSY if interfaces are currently claimed 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_set_configuration(libusb_device_handle dev_handle, int configuration);

		/// <summary>
		/// Claim an interface on a given device handle. You must claim the interface you wish to use before you can perform I/O on any of its endpoints.
		/// It is legal to attempt to claim an already-claimed interface, in which case libusb just returns 0 without doing anything.
		/// If auto_detach_kernel_driver is set to 1 for dev, the kernel driver will be detached if necessary, on failure the detach error is returned.
		/// Claiming of interfaces is a purely logical operation; it does not cause any requests to be sent over the bus. Interface claiming is used to instruct the underlying operating system that your application wishes to take ownership of the interface.
		/// This is a non-blocking function.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="interface_number">the bInterfaceNumber of the interface you wish to claim</param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NOT_FOUND if the requested interface does not exist 
		/// LIBUSB_ERROR_BUSY if another program or driver has claimed the interface 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// a LIBUSB_ERROR code on other failure
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_claim_interface(libusb_device_handle dev_handle, int interface_number);

		/// <summary>
		/// Release an interface previously claimed with libusb_claim_interface(). You should release all claimed interfaces before closing a device handle.
		/// This is a blocking function. A SET_INTERFACE control request will be sent to the device, resetting interface state to the first alternate setting.
		/// If auto_detach_kernel_driver is set to 1 for dev, the kernel driver will be re-attached after releasing the interface.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="interface_number">the bInterfaceNumber of the previously-claimed interface</param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NOT_FOUND if the interface was not claimed 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_release_interface(libusb_device_handle dev_handle, int interface_number);

		/// <summary>
		/// Activate an alternate setting for an interface. The interface must have been previously claimed with libusb_claim_interface().
		/// You should always use this function rather than formulating your own SET_INTERFACE control request. This is because the underlying operating system needs to know when such changes happen.
		/// This is a blocking function.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="interface_number">the bInterfaceNumber of the previously-claimed interface</param>
		/// <param name="alternate_setting">the bAlternateSetting of the alternate setting to activate</param>
		/// <returns>
		/// 0 on success
		/// LIBUSB_ERROR_NOT_FOUND if the interface was not claimed, or the requested alternate setting does not exist 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_set_interface_alt_setting(libusb_device_handle dev_handle, int interface_number, int alternate_setting);

		/// <summary>
		/// Clear the halt/stall condition for an endpoint. Endpoints with halt status are unable to receive or transmit data until the halt condition is stalled.
		/// You should cancel all pending transfers before attempting to clear the halt condition.
		/// This is a blocking function.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="endpoint">the endpoint to clear halt status</param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NOT_FOUND if the endpoint does not exist 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_clear_halt(libusb_device_handle dev_handle, byte endpoint);

		/// <summary>
		/// Perform a USB port reset to reinitialize a device. The system will attempt to restore the previous configuration and alternate settings after the reset has completed.
		/// If the reset fails, the descriptors change, or the previous state cannot be restored, the device will appear to be disconnected and reconnected. This means that the device handle is no longer valid (you should close it) and rediscover the device. A return code of LIBUSB_ERROR_NOT_FOUND indicates when this is the case.
		/// This is a blocking function which usually incurs a noticeable delay.
		/// </summary>
		/// <param name="dev_handle">a handle of the device to reset</param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NOT_FOUND if re-enumeration is required, or if the device has been disconnected 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_reset_device(libusb_device_handle dev_handle);

		/// <summary>
		/// Determine if a kernel driver is active on an interface. If a kernel driver is active, you cannot claim the interface, and libusb will be unable to perform I/O. This functionality is not available on Windows.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="interface_number">the interface to check</param>
		/// <returns>
		/// 0 if no kernel driver is active 
		/// 1 if a kernel driver is active 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_NOT_SUPPORTED on platforms where the functionality is not available 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_kernel_driver_active(libusb_device_handle dev_handle, int interface_number);

		/// <summary>
		/// Detach a kernel driver from an interface. If successful, you will then be able to claim the interface and perform I/O.
		/// This functionality is not available on Darwin or Windows.
		///Note that libusb itself also talks to the device through a special kernel driver, if this driver is already attached to the device, this call will not detach it and return LIBUSB_ERROR_NOT_FOUND.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="interface_number">the interface to detach the driver from</param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NOT_FOUND if no kernel driver was active 
		/// LIBUSB_ERROR_INVALID_PARAM if the interface does not exist 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_NOT_SUPPORTED on platforms where the functionality is not available 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_detach_kernel_driver(libusb_device_handle dev_handle, int interface_number);

		/// <summary>
		/// Re-attach an interface's kernel driver, which was previously detached using libusb_detach_kernel_driver(). This call is only effective on Linux and returns LIBUSB_ERROR_NOT_SUPPORTED on all other platforms.
		/// This functionality is not available on Darwin or Windows.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="interface_number">the interface to attach the driver from </param>
		/// <returns>
		/// 0 on success 
		/// LIBUSB_ERROR_NOT_FOUND if no kernel driver was active 
		/// LIBUSB_ERROR_INVALID_PARAM if the interface does not exist 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_NOT_SUPPORTED on platforms where the functionality is not available 
		/// LIBUSB_ERROR_BUSY if the driver cannot be attached because the interface is claimed by a program or driver 
		/// another LIBUSB_ERROR code on other failure 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_attach_kernel_driver(libusb_device_handle dev_handle, int interface_number);

		/// <summary>
		/// Enable/disable libusb's automatic kernel driver detachment. When this is enabled libusb will automatically detach the kernel driver on an interface when claiming the interface, and attach it when releasing the interface.
		/// Automatic kernel driver detachment is disabled on newly opened device handles by default.
		/// On platforms which do not have LIBUSB_CAP_SUPPORTS_DETACH_KERNEL_DRIVER this function will return LIBUSB_ERROR_NOT_SUPPORTED, and libusb will continue as if this function was never called.
		/// </summary>
		/// <param name="dev_handle">a device handle</param>
		/// <param name="enable">whether to enable or disable auto kernel driver detachment</param>
		/// <returns>
		/// LIBUSB_SUCCESS on success 
		/// LIBUSB_ERROR_NOT_SUPPORTED on platforms where the functionality is not available 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_set_auto_detach_kernel_driver(libusb_device_handle dev_handle, int enable);

		/// <summary>
		/// Perform a USB control transfer.
		/// The direction of the transfer is inferred from the bmRequestType field of the setup packet.
		/// The wValue, wIndex and wLength fields values should be given in host-endian byte order.
		/// </summary>
		/// <param name="dev_handle">a handle for the device to communicate with</param>
		/// <param name="bmRequestType">the request type field for the setup packet</param>
		/// <param name="bRequest">the request field for the setup packet</param>
		/// <param name="wValue">the value field for the setup packet</param>
		/// <param name="wIndex">the index field for the setup packet</param>
		/// <param name="data">a suitably-sized data buffer for either input or output (depending on direction bits within bmRequestType)</param>
		/// <param name="wLength">the length field for the setup packet. The data buffer should be at least this size.</param>
		/// <param name="timeout">timeout (in millseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0. </param>
		/// <returns>
		/// on success, the number of bytes actually transferred 
		/// LIBUSB_ERROR_TIMEOUT if the transfer timed out 
		/// LIBUSB_ERROR_PIPE if the control request was not supported by the device 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_BUSY if called from event handling context 
		/// LIBUSB_ERROR_INVALID_PARAM if the transfer size is larger than the operating system and/or hardware can support 
		/// another LIBUSB_ERROR code on other failures 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_control_transfer(libusb_device_handle dev_handle, byte bmRequestType, byte bRequest, ushort wValue, ushort wIndex, byte[] data, ushort wLength, uint timeout);

		public static int libusb_control_transfer(libusb_device_handle dev_handle, byte bmRequestType, byte bRequest, ushort wValue, ushort wIndex, byte[] data, uint timeout)
		{
			if (data == null) throw new Exception("data can't be null!");
			return libusb_control_transfer(dev_handle, bmRequestType, bRequest, wValue, wIndex, data, (ushort)data.Length, timeout);
		}


		/// <summary>
		/// Perform a USB bulk transfer. The direction of the transfer is inferred from the direction bits of the endpoint address.
		/// For bulk reads, the length field indicates the maximum length of data you are expecting to receive. If less data arrives than expected, this function will return that data, so be sure to check the transferred output parameter.
		/// You should also check the transferred parameter for bulk writes. Not all of the data may have been written.
		/// Also check transferred when dealing with a timeout error code. libusb may have to split your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the timeout may expire after the first few chunks have completed. libusb is careful not to lose any data that may have been transferred; do not assume that timeout conditions indicate a complete lack of I/O.
		/// </summary>
		/// <param name="dev_handle">a handle for the device to communicate with</param>
		/// <param name="endpoint">the address of a valid endpoint to communicate with</param>
		/// <param name="data">a suitably-sized data buffer for either input or output (depending on endpoint)</param>
		/// <param name="length">for bulk writes, the number of bytes from data to be sent. for bulk reads, the maximum number of bytes to receive into the data buffer. </param>
		/// <param name="transferred">output location for the number of bytes actually transferred. Since version 1.0.21 (LIBUSB_API_VERSION >= 0x01000105), it is legal to pass a NULL pointer if you do not wish to receive this information.</param>
		/// <param name="timeout">timeout (in millseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
		/// <returns>
		/// 0 on success (and populates transferred) 
		/// LIBUSB_ERROR_TIMEOUT if the transfer timed out (and populates transferred) 
		/// LIBUSB_ERROR_PIPE if the endpoint halted 
		/// LIBUSB_ERROR_OVERFLOW if the device offered more data, see Packets and overflows 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_BUSY if called from event handling context 
		/// another LIBUSB_ERROR code on other failures 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_bulk_transfer(libusb_device_handle dev_handle, byte endpoint, byte[] data, ushort length, out int transferred, uint timeout);
		/// <summary>
		/// Perform a USB bulk transfer. The direction of the transfer is inferred from the direction bits of the endpoint address.
		/// For bulk reads, the length field indicates the maximum length of data you are expecting to receive. If less data arrives than expected, this function will return that data, so be sure to check the transferred output parameter.
		/// You should also check the transferred parameter for bulk writes. Not all of the data may have been written.
		/// Also check transferred when dealing with a timeout error code. libusb may have to split your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the timeout may expire after the first few chunks have completed. libusb is careful not to lose any data that may have been transferred; do not assume that timeout conditions indicate a complete lack of I/O.
		/// </summary>
		/// <param name="dev_handle">a handle for the device to communicate with</param>
		/// <param name="endpoint">the address of a valid endpoint to communicate with</param>
		/// <param name="data">a suitably-sized data buffer for either input or output (depending on endpoint)</param>
		/// <param name="length">for bulk writes, the number of bytes from data to be sent. for bulk reads, the maximum number of bytes to receive into the data buffer. </param>
		/// <param name="transferred">output location for the number of bytes actually transferred. Since version 1.0.21 (LIBUSB_API_VERSION >= 0x01000105), it is legal to pass a NULL pointer if you do not wish to receive this information.</param>
		/// <param name="timeout">timeout (in millseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
		/// <returns>
		/// 0 on success (and populates transferred) 
		/// LIBUSB_ERROR_TIMEOUT if the transfer timed out (and populates transferred) 
		/// LIBUSB_ERROR_PIPE if the endpoint halted 
		/// LIBUSB_ERROR_OVERFLOW if the device offered more data, see Packets and overflows 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_BUSY if called from event handling context 
		/// another LIBUSB_ERROR code on other failures 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_bulk_transfer(libusb_device_handle dev_handle, byte endpoint, ref byte data, ushort length, out int transferred, uint timeout);

		/// <summary>
		/// Perform a USB bulk transfer. The direction of the transfer is inferred from the direction bits of the endpoint address.
		/// For bulk reads, the length field indicates the maximum length of data you are expecting to receive. If less data arrives than expected, this function will return that data, so be sure to check the transferred output parameter.
		/// You should also check the transferred parameter for bulk writes. Not all of the data may have been written.
		/// Also check transferred when dealing with a timeout error code. libusb may have to split your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the timeout may expire after the first few chunks have completed. libusb is careful not to lose any data that may have been transferred; do not assume that timeout conditions indicate a complete lack of I/O.
		/// </summary>
		/// <param name="dev_handle">a handle for the device to communicate with</param>
		/// <param name="endpoint">the address of a valid endpoint to communicate with</param>
		/// <param name="data">a suitably-sized data buffer for either input or output (depending on endpoint)</param>
		/// <param name="timeout">timeout (in millseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
		/// <returns>
		/// 0 on success (and populates transferred) 
		/// LIBUSB_ERROR_TIMEOUT if the transfer timed out (and populates transferred) 
		/// LIBUSB_ERROR_PIPE if the endpoint halted 
		/// LIBUSB_ERROR_OVERFLOW if the device offered more data, see Packets and overflows 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_BUSY if called from event handling context 
		/// another LIBUSB_ERROR code on other failures 
		/// </returns>
		public static int libusb_bulk_transfer(libusb_device_handle dev_handle, byte endpoint, byte[] data, uint timeout)
		{
			if (data == null) throw new Exception("data can't be null!");
			return libusb_bulk_transfer(dev_handle, endpoint, data, (ushort)data.Length, out _, timeout);
		}

		/// <summary>
		/// Perform a USB bulk transfer. The direction of the transfer is inferred from the direction bits of the endpoint address.
		/// For bulk reads, the length field indicates the maximum length of data you are expecting to receive. If less data arrives than expected, this function will return that data, so be sure to check the transferred output parameter.
		/// You should also check the transferred parameter for bulk writes. Not all of the data may have been written.
		/// Also check transferred when dealing with a timeout error code. libusb may have to split your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the timeout may expire after the first few chunks have completed. libusb is careful not to lose any data that may have been transferred; do not assume that timeout conditions indicate a complete lack of I/O.
		/// </summary>
		/// <param name="dev_handle">a handle for the device to communicate with</param>
		/// <param name="endpoint">the address of a valid endpoint to communicate with</param>
		/// <param name="data">a suitably-sized data buffer for either input or output (depending on endpoint)</param>
		/// <param name="timeout">timeout (in millseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
		/// <returns>
		/// 0 on success (and populates transferred) 
		/// LIBUSB_ERROR_TIMEOUT if the transfer timed out (and populates transferred) 
		/// LIBUSB_ERROR_PIPE if the endpoint halted 
		/// LIBUSB_ERROR_OVERFLOW if the device offered more data, see Packets and overflows 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_BUSY if called from event handling context 
		/// another LIBUSB_ERROR code on other failures 
		/// </returns>
		public static int libusb_bulk_transfer(libusb_device_handle dev_handle, byte endpoint, string str, uint timeout)
		{
			var data = Encoding.ASCII.GetBytes(str);
			return libusb_bulk_transfer(dev_handle, endpoint, data, (ushort)data.Length, out _, timeout);
		}
		/// <summary>
		/// Perform a USB interrupt transfer. The direction of the transfer is inferred from the direction bits of the endpoint address.
		/// For interrupt reads, the length field indicates the maximum length of data you are expecting to receive. If less data arrives than expected, this function will return that data, so be sure to check the transferred output parameter.
		/// You should also check the transferred parameter for interrupt writes. Not all of the data may have been written.
		/// Also check transferred when dealing with a timeout error code. libusb may have to split your transfer into a number of chunks to satisfy underlying O/S requirements, meaning that the timeout may expire after the first few chunks have completed. libusb is careful not to lose any data that may have been transferred; do not assume that timeout conditions indicate a complete lack of I/O.
		/// The default endpoint bInterval value is used as the polling interval.
		/// </summary>
		/// <param name="dev_handle">a handle for the device to communicate with</param>
		/// <param name="endpoint">the address of a valid endpoint to communicate with</param>
		/// <param name="data">a suitably-sized data buffer for either input or output (depending on endpoint)</param>
		/// <param name="length">for bulk writes, the number of bytes from data to be sent. for bulk reads, the maximum number of bytes to receive into the data buffer.</param>
		/// <param name="transferred">output location for the number of bytes actually transferred. Since version 1.0.21 (LIBUSB_API_VERSION >= 0x01000105), it is legal to pass a NULL pointer if you do not wish to receive this information.</param>
		/// <param name="timeout">timeout (in millseconds) that this function should wait before giving up due to no response being received. For an unlimited timeout, use value 0.</param>
		/// <returns>
		/// 0 on success (and populates transferred) 
		/// LIBUSB_ERROR_TIMEOUT if the transfer timed out 
		/// LIBUSB_ERROR_PIPE if the endpoint halted 
		/// LIBUSB_ERROR_OVERFLOW if the device offered more data, see Packets and overflows 
		/// LIBUSB_ERROR_NO_DEVICE if the device has been disconnected 
		/// LIBUSB_ERROR_BUSY if called from event handling context 
		/// another LIBUSB_ERROR code on other error 
		/// </returns>
		[DllImport(LIBUSB, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int libusb_interrupt_transfer(libusb_device_handle dev_handle, byte endpoint, byte[] data, ushort length, out int transferred, uint timeout);


	}//End Class
}
