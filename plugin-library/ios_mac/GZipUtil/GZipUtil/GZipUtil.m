//
//  GZipUtil.m
//  GZipUtil
//
//  Created by tkyaji on 2016/02/09.
//  Copyright © 2016年 tkyaji. All rights reserved.
//

#import "GZipUtil.h"
#import <zlib.h>


#define BUFFER_SIZE 8192

@implementation GZipUtil

+ (NSData *)compress:(NSData *)data {
    
    z_stream zStream;
    zStream.zalloc = Z_NULL;
    zStream.zfree = Z_NULL;
    zStream.opaque = Z_NULL;
    
    Bytef buffer[BUFFER_SIZE];
    deflateInit(&zStream, Z_DEFAULT_COMPRESSION);
    zStream.next_in = (Bytef *)[data bytes];
    zStream.avail_in = (uint)[data length];
    int retval = Z_OK;
    
    NSMutableData *ret = [NSMutableData dataWithCapacity:0];
    do {
        zStream.next_out = buffer;
        zStream.avail_out = sizeof(buffer);
        retval = deflate(&zStream, Z_FINISH);
        size_t length = sizeof(buffer) - zStream.avail_out;
        if (length > 0)
            [ret appendBytes:buffer length:length];
    } while (zStream.avail_out != sizeof(buffer));
    deflateEnd(&zStream);
    
    return ret;
}

+ (NSData *)uncompress:(NSData *)data {
    
    z_stream zStream;
    zStream.zalloc = Z_NULL;
    zStream.zfree = Z_NULL;
    zStream.opaque = Z_NULL;
    
    Bytef buffer[BUFFER_SIZE];
    inflateInit(&zStream);
    zStream.next_in = (Bytef *)[data bytes];
    zStream.avail_in = (uint)[data length];
    int retval = Z_OK;
    
    NSMutableData *ret = [NSMutableData dataWithCapacity:0];
    do {
        zStream.next_out = buffer;
        zStream.avail_out = sizeof(buffer);
        retval = inflate(&zStream, Z_FINISH);
        size_t length = sizeof(buffer) - zStream.avail_out;
        if (length > 0)
            [ret appendBytes:buffer length:length];
    } while (zStream.avail_out != sizeof(buffer));
    inflateEnd(&zStream);
    
    return ret;
}

@end


#ifdef __cplusplus
extern "C" {
#endif
    void _GZipUtil_compress (Byte *data, int dataLength, Byte **resultData, int *resultDataLength) {
        NSData *dt = [NSData dataWithBytes:data length:dataLength];
        NSData *conpressedData = [GZipUtil compress:dt];
        
        NSUInteger len = [conpressedData length];
        Byte* byteData = (Byte*)malloc(len);
        memcpy(byteData, [conpressedData bytes], len);
        
        *resultData = byteData;
        *resultDataLength = (int)len;
    }
    
    void _GZipUtil_uncompress (Byte *data, int dataLength, Byte **resultData, int *resultDataLength) {
        NSData *dt = [NSData dataWithBytes:data length:dataLength];
        NSData *uncompressedData = [GZipUtil uncompress:dt];
        
        NSUInteger len = [uncompressedData length];
        Byte* byteData = (Byte*)malloc(len);
        memcpy(byteData, [uncompressedData bytes], len);
        
        *resultData = byteData;
        *resultDataLength = (int)len;
    }
#ifdef __cplusplus
}
#endif
