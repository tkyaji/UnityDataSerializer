#if UNITY_ANDROID && !UNITY_EDITOR

using UnityEngine;
using System.Collections;

public class GZipUtilPlugin {

	public static byte[] Compress(byte[] data) {
		AndroidJavaClass javaClass = new AndroidJavaClass("com.tkyaji.gziputil.GZipUtil");
		return javaClass.CallStatic<byte[]>("compress", data);
	}

	public static byte[] Uncompress(byte[] data) {
		AndroidJavaClass javaClass = new AndroidJavaClass("com.tkyaji.gziputil.GZipUtil");
		return javaClass.CallStatic<byte[]>("uncompress", data);
	}
}

#endif
