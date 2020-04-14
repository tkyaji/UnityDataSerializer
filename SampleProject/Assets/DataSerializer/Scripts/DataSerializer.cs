#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR) || (UNITY_ANDROID && !UNITY_EDITOR)
#define ENABLE_COMPRESS_SERIALIZE_DATA
#endif

using UnityEngine;
using System;
using System.IO;
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
                _instance = new DataSerializer();
            }
            return _instance;
        }
    }

    private const string tmpDirectory = "tmp_data";
    private const string saveDirectory = "serialize_data";

    private static string dataDirectory {
        get {
#if UNITY_ANDROID && !UNITY_EDITOR
            var currentActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var filesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir");
            var dataDir = filesDir.Call<string>("getCanonicalPath");
#else
            var dataDir = Application.persistentDataPath;
#endif
            return dataDir;
        }
    }

    private static string savePath {
        get {
            return Path.Combine(dataDirectory, saveDirectory);
        }
    }

    private static string tmpPath {
        get {
            return Path.Combine(dataDirectory, tmpDirectory);
        }
    }

    private string cryptKey;
    private string cryptIv;
    private bool enableCompression;
    private Dictionary<string, CacheData> cacheDict = new Dictionary<string, CacheData>();


    public static void SetData(string key, object data, bool withApply = false) {
        instance.setData(key, data);
        if (withApply) {
            instance.apply(key);
        }
    }

    public static T GetData<T>(string key) {
        return instance.getData<T>(key);
    }

    public static T GetData<T>(string key, T defaultValue) {
        return instance.getData<T>(key, defaultValue);
    }

    public static void RemoveData(string key, bool withApply = false) {
        instance.removeData(key);
        if (withApply) {
            instance.apply(key);
        }
    }

    public static void ClearAllData() {
        instance.clearAllData();
    }

    public static void EnableEncryption(string key, string iv = null) {
        instance.enableEncryption(key, iv);
    }

    public static void EnableCompression(bool enable = true) {
        instance.enableCompression = enable;
    }

    public static bool ExistsData(string key) {
        return instance.existsData(key);
    }

    public static void Apply(string key = null) {
        if (key == null) {
            instance.apply();
        } else {
            instance.apply(key);
        }
    }

    public static void PreLoad(params string[] keys) {
        foreach (string key in keys) {
            instance.getData<object>(key);
        }
    }

    public static string GetFilePath(string key) {
        return instance.getFilePath(key);
    }


    private DataSerializer() {
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        if (!Directory.Exists(savePath)) {
            Directory.CreateDirectory(savePath);
        }
        if (!Directory.Exists(tmpPath)) {
            Directory.CreateDirectory(tmpPath);
        }
    }

    private void setData(string key, object data) {
        if (!data.GetType().IsSerializable) {
            throw new ArgumentException("Argument object is not Serializable");
        }

        if (cacheDict.ContainsKey(key)) {
            cacheDict[key].ChangeData(data);

        } else {
            cacheDict[key] = new CacheData(data);
        }
    }

    private T getData<T>(string key) {
        CacheData cacheData = getCacheData(key);
        if (cacheData == null || cacheData.Disabled) {
            return default(T);
        }

        return (T)cacheData.Data;
    }

    private T getData<T>(string key, T defaultValue) {
        if (!existsData(key)) {
            return defaultValue;
        }

        return getData<T>(key);
    }

    private bool existsData(string key) {
        if (cacheDict.ContainsKey(key)) {
            return true;
        }
        string filePath = getFilePath(key);
        return File.Exists(filePath);
    }

    private void removeData(string key) {
        CacheData cacheData = getCacheData(key);
        if (cacheData == null) {
            return;
        }
        cacheData.Disabled = true;
        cacheData.Saved = false;
    }

    private CacheData getCacheData(string key) {
        if (cacheDict.ContainsKey(key)) {
            return cacheDict[key];
        }

        if (!existsData(key)) {
            return null;
        }

        object data = readFile<object>(key);
        cacheDict.Add(key, new CacheData(data, true));

        return cacheDict[key];
    }

    private void clearAllData() {
        string[] files = Directory.GetFiles(savePath);
        foreach (string file in files) {
            File.Delete(file);
        }
        cacheDict.Clear();
    }

    private void enableEncryption(string key, string iv = null) {
        cryptKey = key;
        cryptIv = iv;
    }

    private void apply() {
        List<string> keyList = new List<string>(cacheDict.Keys);
        foreach (string key in keyList) {
            CacheData cacheData = cacheDict[key];
            if (cacheData.Saved) {
                continue;
            }
            if (cacheData.Disabled) {
                deleteFile(key);
                cacheDict.Remove(key);
            } else {
                writeFile(key, cacheData.Data);
                cacheData.Saved = true;
            }
        }
    }

    private void apply(string key) {
        CacheData cacheData = getCacheData(key);
        if (cacheData == null || cacheData.Saved) {
            return;
        }

        if (cacheData.Disabled) {
            deleteFile(key);
            cacheDict.Remove(key);
        } else {
            writeFile(key, cacheData.Data);
            cacheData.Saved = true;
        }
    }

    private T readFile<T>(string key) {
        T data = default(T);

        string filePath = getFilePath(key);
        if (!File.Exists(filePath)) {
            return data;
        }

        byte[] readBytes = File.ReadAllBytes(filePath);
        byte[] bytes = new byte[readBytes.Length - 1];
        Array.Copy(readBytes, 1, bytes, 0, readBytes.Length - 1);

        byte flag = readBytes[0];

        if (cryptKey != null && (flag & (byte)Flags.Crypt) == (byte)Flags.Crypt) {
            bytes = CryptoUtil.Decrypt(bytes, cryptKey, cryptIv);
        }

#if ENABLE_COMPRESS_SERIALIZE_DATA
        if ((flag & (byte)Flags.Compress) == (byte)Flags.Compress) {
            bytes = GZipUtil.Uncompress(bytes);
        }
#endif

        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(bytes)) {
            data = (T)bf.Deserialize(ms);
        }

        return data;
    }

    private void writeFile(string key, object data) {
        string tmpPath = getTmpPath(key);
        string filePath = getFilePath(key);

        using (MemoryStream ms = new MemoryStream()) {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, data);

            byte flag = 0;
            byte[] bytes = ms.ToArray();

#if ENABLE_COMPRESS_SERIALIZE_DATA
            if (enableCompression) {
                bytes = GZipUtil.Compress(bytes);
                flag |= (byte)Flags.Compress;
            }
#endif

            if (cryptKey != null) {
                bytes = CryptoUtil.Encrypt(bytes, cryptKey, cryptIv);
                flag |= (byte)Flags.Crypt;
            }

            byte[] writeBytes = new byte[bytes.Length + 1];
            writeBytes[0] = flag;
            Array.Copy(bytes, 0, writeBytes, 1, bytes.Length);
            try {
                File.WriteAllBytes(tmpPath, writeBytes);
                File.Delete(filePath);
                File.Move(tmpPath, filePath);

            } catch (Exception ex) {
                Debug.LogError("Filed to write file : key=" + key);
                Debug.LogError(ex.Message);
                throw ex;

            } finally {
                if (File.Exists(tmpPath)) {
                    File.Delete(tmpPath);
                }
            }
        }
    }

    private void deleteFile(string key) {
        string filePath = getFilePath(key);
        File.Delete(filePath);
    }

    private string getFilePath(string key) {
        return Path.Combine(savePath, CryptoUtil.Hash(key, CryptoUtil.HashAlgorithm.SHA256));
    }

    private string getTmpPath(string key) {
        return Path.Combine(tmpPath, CryptoUtil.Hash(key, CryptoUtil.HashAlgorithm.SHA256));
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
