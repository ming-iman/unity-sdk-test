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
