using System;
using System.Runtime.InteropServices;

namespace LibUSB.Structures.Native
{
	/// <summary>
	/// A structure representing the standard USB endpoint descriptor. This
	/// descriptor is documented in section 9.6.6 of the USB 3.0 specification.
	/// All multiple-byte fields are represented in host-endian format.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class libusb_endpoint_descriptor
	{
		/// <summary>
		/// Size of this descriptor (in bytes)
		/// </summary>
		public byte bLength;

		/// <summary>
		/// Descriptor type. Will have value
		/// libusb_descriptor_type::LIBUSB_DT_ENDPOINT LIBUSB_DT_ENDPOINT in this context.
		/// </summary>
		public byte bDescriptorType;

		/// <summary>
		/// The address of the endpoint described by this descriptor. Bits 0:3 are the endpoint number. Bits 4:6 are reserved. Bit 7 indicates direction, see libusb_endpoint_direction.
		/// </summary>
		public byte bEndpointAddress;

		/// <summary>
		/// Attributes which apply to the endpoint when it is configured using the bConfigurationValue. Bits 0:1 determine the transfer type and correspond to \ref libusb_transfer_type. Bits 2:3 are only used for isochronous endpoints and correspond to \ref libusb_iso_sync_type. Bits 4:5 are also only used for isochronous endpoints and correspond to libusb_iso_usage_type. Bits 6:7 are reserved.
		/// </summary>
		public byte bmAttributes;

		/// <summary>
		/// Maximum packet size this endpoint is capable of sending/receiving.
		/// </summary>
		public ushort wMaxPacketSize;

		/// <summary>
		/// Interval for polling endpoint for data transfers.
		/// </summary>
		public byte bInterval;

		/// <summary>
		/// For audio devices only: the rate at which synchronization feedback is provided.
		/// </summary>
		public byte bRefresh;

		/// <summary>
		/// For audio devices only: the address if the synch endpoint
		/// </summary>
		public byte bSynchAddress;

		/// <summary>
		/// Extra descriptors. If libusb encounters unknown endpoint descriptors, it will store them here, should you wish to parse them.
		/// </summary>
		//const unsigned char *
		public IntPtr extra;

		/// <summary>
		/// Length of the extra descriptors, in bytes. Must be non-negative.
		/// </summary>
		public int extra_length;
	}
}