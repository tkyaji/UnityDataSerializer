#if !UNITY_EDITOR_OSX && !UNITY_IOS && !UNITY_ANDROID

using System;
using System.Runtime.InteropServices;
using System.Collections;

public class GZipUtilPlugin {
	
	public static byte[] Compress(byte[] data) {
		Debug.LogError ("This platform does not support compression");
		return data;
	}

	public static byte[] Uncompress(byte[] data) {
		Debug.LogError ("This platform does not support compression");
		return data;
	}

}

#endif
