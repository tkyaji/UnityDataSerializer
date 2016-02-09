#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR) || (UNITY_ANDROID && !UNITY_EDITOR)
#define ENABLE_COMPRESS_SERIALIZE_DATA
#endif

using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class DataSerializer {

	[Flags]
	private enum Flags : byte {
		Crypt = 1,
		Compress = 2,
	}

	private static DataSerializer _instance;
	private static DataSerializer instance {
		get {
			if (_instance == null) {
				_instance = new DataSerializer ();
			}
			return _instance;
		}
	}

	private const string saveDirectory = "serialize_data";

	private string savePath;
	private string cryptKey;
	private string cryptIv;
	private Dictionary<string, CacheData> cacheDict = new Dictionary<string, CacheData> ();


	public static void SaveData(string key, object data, bool withApply = false) {
		instance.saveData (key, data);
		if (withApply) {
			instance.apply (key);
		}
	}

	public static T GetData<T>(string key) {
		return instance.getData<T> (key);
	}

	public static T GetData<T>(string key, T defaultValue) {
		return instance.getData<T> (key, defaultValue);
	}

	public static void RemoveData(string key, bool withApply = false) {
		instance.removeData (key);
		if (withApply) {
			instance.apply (key);
		}
	}

	public static void ClearAllData() {
		instance.clearAllData ();
	}

	public static void EnableEncryption(string key, string iv = null) {
		instance.enableEncryption (key, iv);
	}

	public static bool ExistsData(string key) {
		return instance.existsData (key);
	}

	public static void Apply(string key = null) {
		if (key == null) {
			instance.apply ();
		} else {
			instance.apply (key);
		}
	}

	public static void PreLoad(params string[] keys) {
		foreach (string key in keys) {
			instance.getData<object> (key);
		}
	}


	private DataSerializer() {
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
		savePath = Path.Combine (Application.persistentDataPath, saveDirectory);
		if (!Directory.Exists (savePath)) {
			Directory.CreateDirectory (savePath);
		}
	}

	private void saveData(string key, object data) {

		if (!data.GetType().IsSerializable) {
			throw new ArgumentException ("Argument object is not Serializable");
		}

		if (cacheDict.ContainsKey (key)) {
			cacheDict [key].ChangeData (data);

		} else {
			cacheDict [key] = new CacheData (data);
		}
	}

	private T getData<T>(string key) {

		if (cacheDict.ContainsKey (key)) {
			CacheData cacheData = cacheDict [key];
			if (cacheData.Disabled) {
				return default(T);
			} else {
				return (T)cacheData.Data;
			}
		}

		if (!existsData(key)) {
			return default(T);
		}
		T data = readFile<T> (key);

		cacheDict.Add (key, new CacheData (data, true));

		return data;
	}

	private T getData<T>(string key, T defaultValue) {

		if (!existsData (key)) {
			return defaultValue;
		}

		return getData<T> (key);
	}

	private bool existsData(string key) {
		if (cacheDict.ContainsKey (key)) {
			return true;
		}
		string filePath = getFilePath (key);
		return File.Exists (filePath);
	}

	private void removeData(string key) {
		if (cacheDict.ContainsKey (key)) {
			cacheDict [key].Disabled = true;
		}
	}

	private void clearAllData() {
		string[] files = Directory.GetFiles (savePath);
		foreach (string file in files) {
			File.Delete (file);
		}
		cacheDict.Clear ();
	}

	private void enableEncryption(string key, string iv) {
		cryptKey = key;
		cryptIv = iv;
	}

	private void apply() {
		foreach (KeyValuePair<string, CacheData> pair in cacheDict) {
			if (pair.Value.Saved) {
				continue;
			}
			if (pair.Value.Disabled) {
				deleteFile (pair.Key);
				cacheDict.Remove (pair.Key);
			} else {
				writeFile (pair.Key, pair.Value.Data);
				pair.Value.Saved = true;
			}
		}
	}

	private void apply(string key) {
		if (!cacheDict.ContainsKey (key)) {
			return;
		}

		CacheData cacheData = cacheDict [key];
		if (cacheData.Saved) {
			return;
		}
		if (cacheData.Disabled) {
			deleteFile (key);
			cacheDict.Remove (key);
		} else {
			writeFile (key, cacheData.Data);
			cacheData.Saved = true;
		}
	}

	private T readFile<T>(string key) {
		T data = default(T);

		string filePath = getFilePath (key);
		if (!File.Exists (filePath)) {
			return data;
		}

		byte[] readBytes = File.ReadAllBytes (filePath);
		byte[] bytes = new byte[readBytes.Length - 1];
		Array.Copy (readBytes, 1, bytes, 0, readBytes.Length - 1);

		byte flag = readBytes [0];

		if (cryptKey != null && (flag & (byte)Flags.Crypt) == (byte)Flags.Crypt) {
			bytes = CryptoUtil.Decrypt (bytes, cryptKey, cryptIv);
		}

#if ENABLE_COMPRESS_SERIALIZE_DATA
		if ((flag & (byte)Flags.Compress) == (byte)Flags.Compress) {
			bytes = GZipUtil.Uncompress(bytes);
		}
#endif

		BinaryFormatter bf = new BinaryFormatter ();
		using (MemoryStream ms = new MemoryStream (bytes)) {
			data = (T)bf.Deserialize (ms);
		}

		return data;
	}

	private void writeFile(string key, object data) {
		string filePath = getFilePath (key);

		using (MemoryStream ms = new MemoryStream ()) {
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (ms, data);

			byte flag = 0;
			byte[] bytes = ms.ToArray ();

#if ENABLE_COMPRESS_SERIALIZE_DATA
			bytes = GZipUtil.Compress(bytes);
			flag |= (byte)Flags.Compress;
#endif

			if (cryptKey != null) {
				bytes = CryptoUtil.Encrypt (bytes, cryptKey, cryptIv);
				flag |= (byte)Flags.Crypt;
			}

			byte[] writeBytes = new byte[bytes.Length + 1];
			writeBytes [0] = flag;
			Array.Copy (bytes, 0, writeBytes, 1, bytes.Length);
			File.WriteAllBytes (filePath, writeBytes);
		}
	}

	private void deleteFile(string key) {
		string filePath = getFilePath (key);
		File.Delete (filePath);
	}

	private string getFilePath(string key) {
		return Path.Combine (savePath, CryptoUtil.Hash (key, CryptoUtil.HashAlgorithm.SHA256));
	}


	private class CacheData {
		public object Data;
		public bool Saved = false;
		public bool Disabled = false;

		public CacheData(object data, bool saved = false) {
			this.Data = data;
			this.Saved = saved;
		}

		public void ChangeData(object data) {
			this.Data = data;
			this.Saved = false;
			this.Disabled = false;
		}

	}
}
