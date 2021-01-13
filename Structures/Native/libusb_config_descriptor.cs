using System;
using System.Runtime.InteropServices;

namespace LibUSB.Structures.Native
{
	/// <summary>
	/// A structure representing the standard USB configuration descriptor. This
	/// descriptor is documented in section 9.6.3 of the USB 3.0 specification.
	/// All multiple-byte fields are represented in host-endian format.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class libusb_config_descriptor
	{
		/// <summary>
		/// Size of this descriptor (in bytes)
		/// </summary>
		public byte bLength;

		/// <summary>
		/// Descriptor type. Will have value libusb_descriptor_type::LIBUSB_DT_CONFIG LIBUSB_DT_CONFIG in this context.
		/// </summary>
		public byte bDescriptorType;

		/// <summary>
		/// Total length of data returned for this configuration
		/// </summary>
		public ushort wTotalLength;

		/// <summary>
		/// Number of interfaces supported by this configuration
		/// </summary>
		public byte bNumInterfaces;

		/// <summary>
		/// Identifier value for this configuration
		/// </summary>
		public byte bConfigurationValue;

		/// <summary>
		/// Index of string descriptor describing this configuration
		/// </summary>
		public byte iConfiguration;

		/// <summary>
		/// Configuration characteristics
		/// </summary>
		public byte bmAttributes;

		/// <summary>
		/// Maximum power consumption of the USB device from this bus in this
		/// configuration when the device is fully operation. Expressed in units
		/// of 2 mA when the device is operating in high-speed mode and in units
		/// of 8 mA when the device is operating in super-speed mode.
		/// </summary>
		public byte MaxPower;

		/// <summary>
		/// Array of interfaces supported by this configuration. The length of this array is determined by the bNumInterfaces field.
		/// </summary>
		//[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public IntPtr linterface;

		/// <summary>
		/// Array of interfaces supported by this configuration. The length of this array is determined by the bNumInterfaces field.
		/// </summary>
		/// <value></value>
		public libusb_interface[] Linterface
		{
			get
			{
				var tmp = new libusb_interface[bNumInterfaces];
				var p = linterface;
				if (p == null) return null;
				for (int i = 0; i < bNumInterfaces; i++)
				{
					tmp[i] = new libusb_interface();
					Marshal.PtrToStructure(p, tmp[i]);
					p += Marshal.SizeOf(tmp[i]);
				}
				return tmp;
			}
		}

		// */
		/// <summary>
		/// Extra descriptors. If libusb encounters unknown configuration descriptors, it will store them here, should you wish to parse them.
		/// </summary>      
		///const unsigned char *
		public IntPtr extra;
		/// <summary>
		/// Extra descriptors. If libusb encounters unknown configuration descriptors, it will store them here, should you wish to parse them.
		/// </summary>
		/// <value></value>
		public byte[] Extra
		{
			get
			{
				var buf = new byte[extra_length];
				if (extra_length == 0) return buf;
				Marshal.Copy(extra, buf, 0, extra_length);
				return buf;
			}
		}

		/// <summary>
		/// Length of the extra descriptors, in bytes. Must be non-negative.
		/// </summary>
		public int extra_length;
	}
}