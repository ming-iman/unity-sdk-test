#import <Foundation/Foundation.h>

static inline NSString *MSPUnityStringFromCString(const char *value, NSString *fallback) {
    if (value == NULL) {
        return fallback;
    }
    return [NSString stringWithUTF8String:value] ?: fallback;
}

static inline const char *MSPUnityCStringFromString(NSString *value) {
    NSString *safeValue = value ?: @"";
    return strdup(safeValue.UTF8String);
}
