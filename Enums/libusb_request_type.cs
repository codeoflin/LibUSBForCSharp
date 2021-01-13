namespace LibUSB.Enums
{

	public enum libusb_request_type
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		LIBUSB_REQUEST_TYPE_STANDARD = (0x00 << 5),
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		LIBUSB_REQUEST_TYPE_CLASS = (0x01 << 5),
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		LIBUSB_REQUEST_TYPE_VENDOR = (0x02 << 5),
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		LIBUSB_REQUEST_TYPE_RESERVED = (0x03 << 5)
	}
}
