//
//  GZipUtil.h
//  GZipUtil
//
//  Created by tkyaji on 2016/02/09.
//  Copyright © 2016年 tkyaji. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface GZipUtil : NSObject
@end


#ifdef __cplusplus
extern "C" {
#endif
    void _GZipUtil_compress (Byte *data, int dataLength, Byte **resultData, int *resultDataLength);
    void _GZipUtil_uncompress (Byte *data, int dataLength, Byte **resultData, int *resultDataLength);
#ifdef __cplusplus
}
#endif
