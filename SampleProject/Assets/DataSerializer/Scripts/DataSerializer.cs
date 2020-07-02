#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR) || (UNITY_ANDROID && !UNITY_EDITOR)
#define ENABLE_COMPRESS_SERIALIZE_DATA
#endif

using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class DataSerializer {

    private static DataSerializerImpl _instance;
    private static DataSerializerImpl instance {
        get {
            if (_instance == null) {
                _instance = new DataSerializerImpl(new DataSerializerConfig());
            }
            return _instance;
        }
    }

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


    public static DataSerializerImpl CreateInstance() {
        return new DataSerializerImpl(new DataSerializerConfig());
    }

    public static void SetData(string key, object data, bool withApply = false) {
        instance.SetData(key, data, withApply);
    }

    public static T GetData<T>(string key) {
        return instance.GetData<T>(key);
    }

    public static T GetData<T>(string key, T defaultValue) {
        return instance.GetData<T>(key, defaultValue);
    }

    public static void RemoveData(string key, bool withApply = false) {
        instance.RemoveData(key, withApply);
    }

    public static void ClearAllData() {
        instance.ClearAllData();
    }

    public static void EnableEncryption(string key, string iv = null) {
        instance.EnableEncryption(key, iv);
    }

    public static void EnableCompression(bool enable = true) {
        instance.EnableCompression(enable);
    }

    public static bool ExistsData(string key) {
        return instance.ExistsData(key);
    }

    public static void Apply(string key = null) {
        if (key == null) {
            instance.Apply();
        } else {
            instance.Apply(key);
        }
    }

    public static void PreLoad(params string[] keys) {
        instance.PreLoad(keys);
    }

    public static string GetFilePath(string key) {
        return Path.Combine(savePath, CryptoUtil.Hash(key, CryptoUtil.HashAlgorithm.SHA256));
    }

    public static T ReadFile<T>(string filePath, DataSerializerConfig config) {
        if (!File.Exists(filePath)) {
            return default(T);
        }

        byte[] bytes = File.ReadAllBytes(filePath);
        return BytesToData<T>(bytes, config);
    }

    public static T BytesToData<T>(byte[] bytes, DataSerializerConfig config) {
        byte[] dataBytes = new byte[bytes.Length - 1];
        Array.Copy(bytes, 1, dataBytes, 0, bytes.Length - 1);

        byte flag = bytes[0];

        if (config.CryptKey != null && (flag & (byte)Flags.Crypt) == (byte)Flags.Crypt) {
            dataBytes = CryptoUtil.Decrypt(bytes, config.CryptKey, config.CryptIv);
        }

#if ENABLE_COMPRESS_SERIALIZE_DATA
        if ((flag & (byte)Flags.Compress) == (byte)Flags.Compress) {
            dataBytes = GZipUtil.Uncompress(dataBytes);
        }
#endif

        T data = default(T);
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(dataBytes)) {
            data = (T)bf.Deserialize(ms);
        }

        return data;
    }

    public static void WriteFile(object data, string filePath, DataSerializerConfig config) {
        string tmpFilePath = filePath + ".tmp";
        byte flag;
        var bytes = DataToBytes(data, config, out flag);

        byte[] writeBytes = new byte[bytes.Length + 1];
        writeBytes[0] = flag;
        Array.Copy(bytes, 0, writeBytes, 1, bytes.Length);
        try {
            File.WriteAllBytes(tmpFilePath, writeBytes);
            File.Delete(filePath);
            File.Move(tmpFilePath, filePath);

        } catch (Exception ex) {
            Debug.LogError("Filed to write file : filepath=" + filePath);
            Debug.LogError(ex.Message);
            throw ex;

        } finally {
            if (File.Exists(tmpFilePath)) {
                File.Delete(tmpFilePath);
            }
        }
    }

    public static byte[] DataToBytes(object data, DataSerializerConfig config) {
        byte dummy;
        return DataToBytes(data, config, out dummy);
    }

    public static byte[] DataToBytes(object data, DataSerializerConfig config, out byte flag) {
        byte[] bytes = null;
        flag = 0;
        using (MemoryStream ms = new MemoryStream()) {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, data);

            bytes = ms.ToArray();

#if ENABLE_COMPRESS_SERIALIZE_DATA
            if (config.EnableCompression) {
                bytes = GZipUtil.Compress(bytes);
                flag |= (byte)Flags.Compress;
            }
#endif

            if (config.CryptKey != null) {
                bytes = CryptoUtil.Encrypt(bytes, config.CryptKey, config.CryptIv);
                flag |= (byte)Flags.Crypt;
            }
        }
        return bytes;
    }


    [Flags]
    private enum Flags : byte {
        Crypt = 1,
        Compress = 2,
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


    public class DataSerializerConfig {
        public string CryptKey = null;
        public string CryptIv = null;
        public bool EnableCompression = false;
    }


    public class DataSerializerImpl {

        private DataSerializerConfig config;
        private Dictionary<string, CacheData> cacheDict = new Dictionary<string, CacheData>();

        public DataSerializerImpl(DataSerializerConfig config) {
            this.config = config;
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            if (!Directory.Exists(savePath)) {
                Directory.CreateDirectory(savePath);
            }
        }

        public void SetData(string key, object data, bool withApply = false) {
            if (!data.GetType().IsSerializable) {
                throw new ArgumentException("Argument object is not Serializable");
            }

            if (cacheDict.ContainsKey(key)) {
                cacheDict[key].ChangeData(data);

            } else {
                cacheDict[key] = new CacheData(data);
            }

            if (withApply) this.Apply(key);
        }

        public T GetData<T>(string key) {
            CacheData cacheData = getCacheData(key);
            if (cacheData == null || cacheData.Disabled) {
                return default(T);
            }

            return (T)cacheData.Data;
        }

        public T GetData<T>(string key, T defaultValue) {
            if (!ExistsData(key)) {
                return defaultValue;
            }

            return GetData<T>(key);
        }

        public bool ExistsData(string key) {
            if (cacheDict.ContainsKey(key)) {
                return true;
            }
            string filePath = GetFilePath(key);
            return File.Exists(filePath);
        }

        public void RemoveData(string key, bool withApply = false) {
            CacheData cacheData = getCacheData(key);
            if (cacheData == null) {
                return;
            }
            cacheData.Disabled = true;
            cacheData.Saved = false;

            if (withApply) this.Apply(key);
        }

        private CacheData getCacheData(string key) {
            if (cacheDict.ContainsKey(key)) {
                return cacheDict[key];
            }

            if (!ExistsData(key)) {
                return null;
            }

            object data = readFile<object>(key);
            cacheDict.Add(key, new CacheData(data, true));

            return cacheDict[key];
        }

        public void ClearAllData() {
            string[] files = Directory.GetFiles(savePath);
            foreach (string file in files) {
                File.Delete(file);
            }
            cacheDict.Clear();
        }

        public void EnableEncryption(string key, string iv = null) {
            config.CryptKey = key;
            config.CryptIv = iv;
        }

        public void EnableCompression(bool enable = true) {
            config.EnableCompression = enable;
        }

        public void Apply() {
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

        public void Apply(string key) {
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
            string filePath = GetFilePath(key);
            if (!File.Exists(filePath)) {
                return default(T);
            }

            return ReadFile<T>(filePath, config);
        }

        private void writeFile(string key, object data) {
            string filePath = GetFilePath(key);
            WriteFile(data, filePath, config);
        }

        private void deleteFile(string key) {
            string filePath = GetFilePath(key);
            File.Delete(filePath);
        }

        public void PreLoad(params string[] keys) {
            foreach (string key in keys) {
                this.GetData<object>(key);
            }
        }

    }
}
