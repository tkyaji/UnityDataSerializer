//
//  GZipUtilTest.m
//  GZipUtilTest
//
//  Created by tkyaji on 2016/02/09.
//  Copyright © 2016年 tkyaji. All rights reserved.
//

#import <XCTest/XCTest.h>
#import "GZipUtil.h"

@interface GZipUtilTest : XCTestCase

@end

@implementation GZipUtilTest

- (void)setUp {
    [super setUp];
    // Put setup code here. This method is called before the invocation of each test method in the class.
}

- (void)tearDown {
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

- (void)testCompress {

    NSString *str = @"abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz";
    NSData *data = [str dataUsingEncoding:NSUTF8StringEncoding];
    
    NSUInteger len = [data length];
    Byte *byteData = (Byte*)malloc(len);
    memcpy(byteData, [data bytes], len);
    
    Byte *compressedByteData = nil;
    int compressedLen = 0;
    
    _GZipUtil_compress(byteData, (int)len, &compressedByteData, &compressedLen);
    
    Byte *uncompressedByteData = nil;
    int uncompressedLen = 0;
    
    _GZipUtil_uncompress(compressedByteData, compressedLen, &uncompressedByteData, &uncompressedLen);
    
    NSData *resultData = [NSData dataWithBytes:uncompressedByteData length:uncompressedLen];
    NSString *resultStr = [[NSString alloc] initWithData:resultData encoding:NSUTF8StringEncoding];
    
    NSLog(@"compress : %d, %d, %d", (int)len, compressedLen, uncompressedLen);
    NSLog(@"%@", resultStr);
}

@end
