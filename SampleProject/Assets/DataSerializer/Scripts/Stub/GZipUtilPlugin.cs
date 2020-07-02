#if !UNITY_STANDALONE_OSX && !UNITY_EDITOR_OSX && !(UNITY_IOS && !UNITY_EDITOR) && !(UNITY_ANDROID && !UNITY_EDITOR)

using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;

public class GZipUtilPlugin {

	public static byte[] Compress(byte[] data) {
		Debug.LogError("This platform does not support compression");
		return data;
	}

	public static byte[] Uncompress(byte[] data) {
		Debug.LogError("This platform does not support compression");
		return data;
	}

}

#endif
