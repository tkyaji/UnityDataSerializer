#if UNITY_IOS && !UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using System.Collections;

public class GZipUtilPlugin {

	[DllImport("__Internal")]
	private static extern void _GZipUtil_compress(byte[] data, int dataLength, out IntPtr resultData, out int resultDataLength);

	[DllImport("__Internal")]
	private static extern void _GZipUtil_uncompress(byte[] data, int dataLength, out IntPtr resultData, out int resultDataLength);


	public static byte[] Compress(byte[] data) {
		IntPtr ptr = IntPtr.Zero;
		int size = 0;

		_GZipUtil_compress(data, data.Length, out ptr, out size);
		byte[] resultData = new byte[size];
		Marshal.Copy(ptr, resultData, 0, size);

		return resultData;
	}

	public static byte[] Uncompress(byte[] data) {
		IntPtr ptr = IntPtr.Zero;
		int size = 0;

		_GZipUtil_uncompress(data, data.Length, out ptr, out size);
		byte[] resultData = new byte[size];
		Marshal.Copy(ptr, resultData, 0, size);

		return resultData;
	}
}

#endif
