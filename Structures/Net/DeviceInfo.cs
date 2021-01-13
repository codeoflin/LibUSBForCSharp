using System;

namespace LibUSB.Structures.Net
{
	/// <summary>
	/// 
	/// </summary>
	public class DeviceInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public IntPtr Handle;
		/// <summary>
		/// 
		/// </summary>
		public byte EndPoint_In;
		/// <summary>
		/// 
		/// </summary>
		public byte EndPoint_Out;
		/// <summary>
		/// 
		/// </summary>
		public byte InterfaceNumber;
		/// <summary>
		/// 
		/// </summary>
		public ushort VID;
		/// <summary>
		/// 
		/// </summary>
		public ushort PID;
	}
}