using System.IO;
using System.Text;
using System.Security.Cryptography;


public class CryptoUtil {

	public enum HashAlgorithm {
		MD5,
		SHA1,
		SHA256,
	}

	public static byte[] Encrypt(byte[] data, string key, string iv = null) {
		RijndaelManaged aes = createAES(key, iv);
		ICryptoTransform encrypt = aes.CreateEncryptor();
		return encrypt.TransformFinalBlock(data, 0, data.Length);
	}

	public static byte[] Decrypt(byte[] data, string key, string iv = null) {
		RijndaelManaged aes = createAES(key, iv);
		ICryptoTransform decrypt = aes.CreateDecryptor();
		return decrypt.TransformFinalBlock(data, 0, data.Length);
	}

	private static RijndaelManaged createAES(string key, string iv) {
		key = key.PadRight(32, ' ').Substring(0, 32);
		if (iv == null) {
			iv = key;
		}
		iv = iv.PadRight(16, ' ').Substring(0, 16);

		RijndaelManaged aes = new RijndaelManaged();
		aes.BlockSize = 128;
		aes.KeySize = 128;
		aes.IV = Encoding.UTF8.GetBytes(iv);
		aes.Key = Encoding.UTF8.GetBytes(key);
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		return aes;
	}

	public static string Hash(string srcStr, HashAlgorithm algorithm) {

		System.Security.Cryptography.HashAlgorithm hashAlgorithm = null;

		switch (algorithm) {
			case HashAlgorithm.MD5:
				hashAlgorithm = System.Security.Cryptography.MD5.Create();
				break;
			case HashAlgorithm.SHA1:
				hashAlgorithm = System.Security.Cryptography.SHA1.Create();
				break;
			case HashAlgorithm.SHA256:
				hashAlgorithm = System.Security.Cryptography.SHA256.Create();
				break;
		}

		byte[] srcBytes = System.Text.Encoding.UTF8.GetBytes(srcStr);
		byte[] destBytes = hashAlgorithm.ComputeHash(srcBytes);

		System.Text.StringBuilder destStrBuilder;
		destStrBuilder = new System.Text.StringBuilder();
		foreach (byte curByte in destBytes) {
			destStrBuilder.Append(curByte.ToString("x2"));
		}

		return destStrBuilder.ToString();
	}
}
