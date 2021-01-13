namespace LibUSB.Enums
{
	/// <summary>
	/// Recipient bits of the bmRequestType field in control transfers. Values 4 through 31 are reserved. 
	/// </summary>
	public enum libusb_request_recipient
	{
		LIBUSB_RECIPIENT_DEVICE = 0x00,

		LIBUSB_RECIPIENT_INTERFACE = 0x01,

		LIBUSB_RECIPIENT_ENDPOINT = 0x02,

		LIBUSB_RECIPIENT_OTHER = 0x03,
	}
}
