#import <Foundation/Foundation.h>
#import "Bridge/MSPUnityCStringUtils.h"
#import "Bridge/MSPUnityEntry.h"

extern "C" const char* msp_unity_get_version() {
    NSString *version = [MSPUnityEntry sdkVersion] ?: @"";
    return MSPUnityCStringFromString(version);
}

extern "C" void msp_unity_set_log_level(int level) {
    [MSPUnityEntry setLogLevel:(int32_t)level];
}

extern "C" void msp_unity_activate_adapter(const char* adapterId, const char* bootstrapClassName) {
    NSString *adapter = MSPUnityStringFromCString(adapterId, @"");
    NSString *className = MSPUnityStringFromCString(bootstrapClassName, @"");
    [MSPUnityEntry activateAdapterWithAdapterId:adapter bootstrapClassName:className];
}

extern "C" void msp_unity_initialize(const char* prebidApiKey, int orgId, int appId, bool isInTestMode) {
    NSString *apiKey = MSPUnityStringFromCString(prebidApiKey, @"");
    [MSPUnityEntry initializeWithPrebidApiKey:apiKey orgId:(int32_t)orgId appId:(int32_t)appId isInTestMode:isInTestMode];
}

extern "C" void msp_unity_initialize_json(const char* initializationJson) {
    NSString *json = MSPUnityStringFromCString(initializationJson, @"{}");
    [MSPUnityEntry initializeWithJson:json];
}

extern "C" const char* msp_unity_create_ad_loader() {
    NSString *loaderId = [MSPUnityEntry createAdLoader] ?: @"";
    return MSPUnityCStringFromString(loaderId);
}

extern "C" void msp_unity_destroy_ad_loader(const char* loaderId) {
    NSString *lid = MSPUnityStringFromCString(loaderId, @"");
    [MSPUnityEntry destroyAdLoaderWithLoaderId:lid];
}

extern "C" void msp_unity_load_ad(
    const char* loaderId,
    const char* placementId,
    const char* customParamsJson,
    const char* testParamsJson
) {
    NSString *lid = MSPUnityStringFromCString(loaderId, @"");
    NSString *pid = MSPUnityStringFromCString(placementId, @"");
    NSString *customJson = MSPUnityStringFromCString(customParamsJson, @"{}");
    NSString *testJson = MSPUnityStringFromCString(testParamsJson, @"{}");
    [MSPUnityEntry loadAdWithLoaderId:lid
                          placementId:pid
                     customParamsJson:customJson
                       testParamsJson:testJson];
}

extern "C" bool msp_unity_get_ad(const char* loaderId, const char* placementId) {
    NSString *lid = MSPUnityStringFromCString(loaderId, @"");
    NSString *pid = MSPUnityStringFromCString(placementId, @"");
    return [MSPUnityEntry hasAdWithLoaderId:lid placementId:pid];
}

extern "C" void msp_unity_show_ad(const char* loaderId) {
    NSString *lid = MSPUnityStringFromCString(loaderId, @"");
    [MSPUnityEntry showAdWithLoaderId:lid];
}
