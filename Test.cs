using System;
using static LibUSB.Functions.NativeMethods;

namespace LibUSB
{
	internal class Test
	{
		static IntPtr ctx = IntPtr.Zero;
		private static void Main()
		{
			var version = libusb_get_version();
			var iret = libusb_init(out ctx);
			libusb_exit(ctx);
		}
	}
}