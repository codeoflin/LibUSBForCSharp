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
	public class libusb_interface_descriptor
	{
		/// <summary>
		/// Size of this descriptor (in bytes)
		/// </summary>
		public byte bLength;

		/// <summary>
		/// Descriptor type. Will have value libusb_descriptor_type::LIBUSB_DT_INTERFACE LIBUSB_DT_INTERFACE in this context.
		/// </summary>
		public byte bDescriptorType;

		/// <summary>
		/// Number of this interface
		/// </summary>
		public byte bInterfaceNumber;

		/// <summary>
		/// Value used to select this alternate setting for this interface
		/// </summary>
		public byte bAlternateSetting;

		/// <summary>
		/// Number of endpoints used by this interface (excluding the control endpoint).
		/// </summary>
		public byte bNumEndpoints;

		/// <summary>
		/// USB-IF class code for this interface. See libusb_class_code.
		/// </summary>
		public byte bInterfaceClass;

		/// <summary>
		/// USB-IF subclass code for this interface, qualified by the bInterfaceClass value
		/// </summary>
		public byte bInterfaceSubClass;

		/// <summary>
		/// USB-IF protocol code for this interface, qualified by the bInterfaceClass and bInterfaceSubClass values
		/// </summary>
		public byte bInterfaceProtocol;

		/// <summary>
		/// Index of string descriptor describing this interface
		/// </summary>
		public byte iInterface;

		/// <summary>
		/// 
		/// </summary>
		//[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255, ArraySubType = UnmanagedType.Struct)]
		private IntPtr endpoint;
		/// <summary>
		/// Array of endpoint descriptors. This length of this array is determined by the bNumEndpoints field.
		/// </summary>
		/// <value></value>
		public libusb_endpoint_descriptor[] EndPoint
		{
			get
			{
				var tmp = new libusb_endpoint_descriptor[bNumEndpoints];
				var p = endpoint;
				if (p == null) return null;
				for (int i = 0; i < bNumEndpoints; i++)
				{
					tmp[i] = new libusb_endpoint_descriptor();
					Marshal.PtrToStructure(p, tmp[i]);
					p += Marshal.SizeOf(tmp[i]);
				}
				return tmp;
			}
		}

		/// <summary>
		/// Extra descriptors. If libusb encounters unknown interface descriptors, it will store them here, should you wish to parse them.
		/// </summary>
		//const unsigned char *
		public IntPtr extra;
		/// <summary>
		/// Extra descriptors. If libusb encounters unknown interface descriptors, it will store them here, should you wish to parse them.
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