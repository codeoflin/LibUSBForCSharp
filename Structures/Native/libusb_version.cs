using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibUSB.Structures.Native
{
	/// <summary>
	/// 
	/// </summary>
	public struct libusb_version
	{
		/// <summary>
		/// Library major version. 
		/// </summary>
		public ushort major;
		/// <summary>
		/// Library minor version. 
		/// </summary>
		public ushort minor;
		/// <summary>
		/// Library micro version. 
		/// </summary>
		public ushort micro;
		/// <summary>
		/// Library nano version. 
		/// </summary>
		public ushort nano;
		/// <summary>
		/// Library release candidate suffix string,e.g.
		/// "-rc4". 
		/// </summary>
		public IntPtr rc;
		/// <summary>
		/// Library release candidate suffix string,
		/// </summary>
		public string RC
		{
			get => PtrToStringUTF8(rc);
		}
		/// <summary>
		/// For ABI compatibility only. 
		/// </summary>
		public IntPtr describe;
		/// <summary>
		/// For ABI compatibility only. 
		/// </summary>
		/// <value></value>
		public string Describe
		{
			get => PtrToStringUTF8(describe);
		}

		private unsafe static string PtrToStringUTF8(IntPtr pstr)
		{
			var bs = (byte*)pstr;
			var buf = new System.Collections.Generic.List<byte>();
			for (int i = 0; bs[i] != 0; i++) buf.Add(bs[i]);
			return Encoding.UTF8.GetString(buf.ToArray());
			//Marshal.to	
#if NETCOREAPP
		
#else
#endif
		}
	}//End Class
}