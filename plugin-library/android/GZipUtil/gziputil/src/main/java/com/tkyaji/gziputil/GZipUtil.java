package com.tkyaji.gziputil;

import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.util.zip.Deflater;
import java.util.zip.Inflater;

import javax.crypto.Cipher;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

/**
 * Created by tkyaji on 2016/02/09.
 */
public class GZipUtil {

    private static final String TAG = "GZipUtil";
    private static final int BUFFER_SIZE = 8192;

    public static byte[] compress(byte[] data) {
        Deflater deflater = new Deflater();
        deflater.setInput(data);
        deflater.finish();

        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        byte[] buf = new byte[BUFFER_SIZE];
        try {
            while (!deflater.finished()) {
                int byteCount = deflater.deflate(buf);
                baos.write(buf, 0, byteCount);
            }
            deflater.end();
        } catch (Exception ex) {
            Log.e(TAG, ex.getMessage(), ex);
            return null;
        }

        return baos.toByteArray();
    }

    public static byte[] uncompress(byte[] data) {

        Inflater inflater = new Inflater();
        inflater.setInput(data, 0, data.length);

        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        byte[] buf = new byte[BUFFER_SIZE];
        try {
            while (!inflater.finished()) {
                int byteCount = inflater.inflate(buf);
                baos.write(buf, 0, byteCount);
            }
            inflater.end();
        } catch (Exception ex) {
            Log.e(TAG, ex.getMessage(), ex);
            return null;
        }

        return baos.toByteArray();
    }

    public static byte[] encrypt(byte[] data, String key, String iv) {
        return crypt(key, iv, data, Cipher.ENCRYPT_MODE);
    }

    public static byte[] decrypt(byte[] data, String key, String iv) {
        return crypt(key, iv, data, Cipher.DECRYPT_MODE);
    }

    private static byte[] crypt(String key, String iv, byte[] data, int mode) {

        key = String.format("%-32s", key).substring(0, 32);
        iv = String.format("%-16s", iv).substring(0, 16);

        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        try {
            SecretKey skey = new SecretKeySpec(key.getBytes("UTF-8"), "AES");
            Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
            cipher.init(mode, skey, new IvParameterSpec(iv.getBytes("UTF-8")));

            baos.write(cipher.doFinal(data));

        } catch (Exception ex) {
            Log.e(TAG, ex.getMessage(), ex);
            return null;
        }

        return baos.toByteArray();
    }
}
