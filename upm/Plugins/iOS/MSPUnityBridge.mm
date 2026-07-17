#import <Foundation/Foundation.h>

@interface MSPUnityEntry : NSObject
+ (NSString *)sdkVersion;
+ (void)setLogLevel:(int32_t)level;
+ (BOOL)activateAdapterWithAdapterId:(NSString *)adapterId bootstrapClassName:(NSString *)bootstrapClassName;
+ (BOOL)initializeWithPrebidApiKey:(NSString *)prebidApiKey orgId:(int32_t)orgId appId:(int32_t)appId isInTestMode:(BOOL)isInTestMode;
+ (BOOL)initializeWithJson:(NSString *)json;
+ (NSString *)createAdLoader;
+ (void)destroyAdLoaderWithLoaderId:(NSString *)loaderId;
+ (BOOL)loadAdWithLoaderId:(NSString *)loaderId
               placementId:(NSString *)placementId
          customParamsJson:(NSString *)customParamsJson
            testParamsJson:(NSString *)testParamsJson;
+ (BOOL)hasAdWithLoaderId:(NSString *)loaderId placementId:(NSString *)placementId;
+ (BOOL)showAdWithLoaderId:(NSString *)loaderId;
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

extern "C" void msp_unity_initialize_json(const char* initializationJson) {
    NSString *json = initializationJson ? [NSString stringWithUTF8String:initializationJson] : @"{}";
    [MSPUnityEntry initializeWithJson:json];
}

extern "C" const char* msp_unity_create_ad_loader() {
    NSString *loaderId = [MSPUnityEntry createAdLoader] ?: @"";
    return strdup(loaderId.UTF8String);
}

extern "C" void msp_unity_destroy_ad_loader(const char* loaderId) {
    NSString *lid = loaderId ? [NSString stringWithUTF8String:loaderId] : @"";
    [MSPUnityEntry destroyAdLoaderWithLoaderId:lid];
}

extern "C" void msp_unity_load_ad(
    const char* loaderId,
    const char* placementId,
    const char* customParamsJson,
    const char* testParamsJson
) {
    NSString *lid = loaderId ? [NSString stringWithUTF8String:loaderId] : @"";
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    NSString *customJson = customParamsJson ? [NSString stringWithUTF8String:customParamsJson] : @"{}";
    NSString *testJson = testParamsJson ? [NSString stringWithUTF8String:testParamsJson] : @"{}";
    [MSPUnityEntry loadAdWithLoaderId:lid
                          placementId:pid
                     customParamsJson:customJson
                       testParamsJson:testJson];
}

extern "C" bool msp_unity_get_ad(const char* loaderId, const char* placementId) {
    NSString *lid = loaderId ? [NSString stringWithUTF8String:loaderId] : @"";
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    return [MSPUnityEntry hasAdWithLoaderId:lid placementId:pid];
}

extern "C" void msp_unity_show_ad(const char* loaderId) {
    NSString *lid = loaderId ? [NSString stringWithUTF8String:loaderId] : @"";
    [MSPUnityEntry showAdWithLoaderId:lid];
}
