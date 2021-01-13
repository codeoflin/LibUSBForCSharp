
using System.Runtime.InteropServices;
namespace LibUSB.Structures.Native
{
	/// <summary>
	/// A structure representing the standard USB device descriptor. This descriptor is documented in section 9.6.1 of the USB 3.0 specification. All multiple-byte fields are represented in host-endian format.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	public struct libusb_device_descriptor
	{
		/// <summary>
		/// Size of this descriptor (in bytes) 
		/// </summary>
		//[MarshalAs(UnmanagedType.U2, SizeConst = 1)]
		public byte bLength;

		/// <summary>
		/// Descriptor type. Will have value libusb_descriptor_type::LIBUSB_DT_DEVICE LIBUSB_DT_DEVICE in this context.
		/// </summary>
		public byte bDescriptorType;

		/// <summary>
		/// USB specification release number in binary-coded decimal. A value of 0x0200 indicates USB 2.0, 0x0110 indicates USB 1.1, etc.
		/// </summary>
		public ushort bcdUSB;

		/// <summary>
		/// USB-IF class code for the device. See libusb_class_code.
		/// </summary>
		public byte bDeviceClass;

		/// <summary>
		/// USB-IF subclass code for the device, qualified by the bDeviceClass value
		/// </summary>
		public byte bDeviceSubClass;

		/// <summary>
		/// USB-IF protocol code for the device, qualified by the bDeviceClass and bDeviceSubClass values
		/// </summary>
		public byte bDeviceProtocol;

		/// <summary>
		/// Maximum packet size for endpoint 0
		/// </summary>
		public byte bMaxPacketSize0;

		/// <summary>
		/// USB-IF vendor ID
		/// </summary>
		public ushort idVendor;

		/// <summary>
		/// USB-IF product ID
		/// </summary>
		public ushort idProduct;

		/// <summary>
		/// Device release number in binary-coded decimal
		/// </summary>
		public ushort bcdDevice;

		/// <summary>
		/// Index of string descriptor describing manufacturer
		/// </summary>
		public byte iManufacturer;

		/// <summary>
		/// Index of string descriptor describing product
		/// </summary>
		public byte iProduct;

		/// <summary>
		/// Index of string descriptor containing device serial number
		/// </summary>
		public byte iSerialNumber;

		/// <summary>
		/// Number of possible configurations
		/// </summary>
		public byte bNumConfigurations;
	};
}