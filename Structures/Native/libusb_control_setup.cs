namespace LibUSB.Structures.Native
{
	/// <summary>
	/// 
	/// </summary>
	public class libusb_control_setup
	{
		/// <summary>
		/// Request type.
		/// </summary>
		public byte bmRequestType;
		/// <summary>
		/// Request
		/// </summary>
		public byte bRequest;
		/// <summary>
		/// Value
		/// </summary>
		public ushort wValue;
		/// <summary>
		/// Index
		/// </summary>
		public ushort wIndex;
		/// <summary>
		/// Number of bytes to transfer. 
		/// </summary>
		public ushort wLength;
	}
}