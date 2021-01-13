using System;
using System.Runtime.InteropServices;

namespace LibUSB.Structures.Native
{
	/// <summary>
	/// A collection of alternate settings for a particular USB interface.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class libusb_interface
	{
		/// <summary>
		/// Array of interface descriptors. The length of this array is determined by the num_altsetting field.
		/// </summary>
		public IntPtr altsetting;
		/// <summary>
		/// Array of interface descriptors. The length of this array is determined by the num_altsetting field.
		/// </summary>
		/// <value></value>
		public libusb_interface_descriptor[] Altsetting
		{
			get
			{
				var tmp = new libusb_interface_descriptor[num_altsetting];
				var p=altsetting;
				for (int i = 0; i < num_altsetting; i++)
				{
					tmp[i]=new libusb_interface_descriptor();
					Marshal.PtrToStructure(p, tmp[i]);
					p += Marshal.SizeOf(tmp[i]);
				}
				return tmp;
			}
		}

		/// <summary>
		/// The number of alternate settings that belong to this interface. Must be non-negative.
		/// </summary>
		public int num_altsetting;
	}
}
