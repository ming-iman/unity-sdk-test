#import <Foundation/Foundation.h>

@interface MSPUnityEntry : NSObject
+ (NSString *)sdkVersion;
+ (void)setLogLevel:(int32_t)level;
+ (BOOL)activateAdapterWithAdapterId:(NSString *)adapterId bootstrapClassName:(NSString *)bootstrapClassName;
+ (BOOL)initializeWithPrebidApiKey:(NSString *)prebidApiKey orgId:(int32_t)orgId appId:(int32_t)appId isInTestMode:(BOOL)isInTestMode;
+ (BOOL)loadAdWithPlacementId:(NSString *)placementId requestToken:(NSString *)requestToken adNetwork:(NSString *)adNetwork;
+ (BOOL)hasAdWithPlacementId:(NSString *)placementId requestToken:(NSString *)requestToken;
+ (BOOL)showAdWithPlacementId:(NSString *)placementId requestToken:(NSString *)requestToken;
@end

extern "C" const char* msp_unity_get_version() {
    NSString *version = [MSPUnityEntry sdkVersion] ?: @"";
    return strdup(version.UTF8String);
}

extern "C" void msp_unity_set_log_level(int level) {
    [MSPUnityEntry setLogLevel:(int32_t)level];
}

extern "C" void msp_unity_activate_adapter(const char* adapterId, const char* bootstrapClassName) {
    NSString *adapter = adapterId ? [NSString stringWithUTF8String:adapterId] : @"";
    NSString *className = bootstrapClassName ? [NSString stringWithUTF8String:bootstrapClassName] : @"";
    [MSPUnityEntry activateAdapterWithAdapterId:adapter bootstrapClassName:className];
}

extern "C" void msp_unity_initialize(const char* prebidApiKey, int orgId, int appId, bool isInTestMode) {
    NSString *apiKey = prebidApiKey ? [NSString stringWithUTF8String:prebidApiKey] : @"";
    [MSPUnityEntry initializeWithPrebidApiKey:apiKey orgId:(int32_t)orgId appId:(int32_t)appId isInTestMode:isInTestMode];
}

extern "C" void msp_unity_load_ad(const char* placementId, const char* requestToken, const char* adNetwork) {
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    NSString *token = requestToken ? [NSString stringWithUTF8String:requestToken] : @"";
    NSString *network = adNetwork ? [NSString stringWithUTF8String:adNetwork] : @"";
    [MSPUnityEntry loadAdWithPlacementId:pid requestToken:token adNetwork:network];
}

extern "C" bool msp_unity_get_ad(const char* placementId, const char* requestToken) {
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    NSString *token = requestToken ? [NSString stringWithUTF8String:requestToken] : @"";
    return [MSPUnityEntry hasAdWithPlacementId:pid requestToken:token];
}

extern "C" void msp_unity_show_ad(const char* placementId, const char* requestToken) {
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    NSString *token = requestToken ? [NSString stringWithUTF8String:requestToken] : @"";
    [MSPUnityEntry showAdWithPlacementId:pid requestToken:token];
}
