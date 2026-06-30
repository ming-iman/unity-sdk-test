#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <MSPCore/MSPCore-Swift.h>
#import <MSPiOSCore/MSPiOSCore-Swift.h>

extern void UnitySendMessage(const char *obj, const char *method, const char *msg);

static NSString *const kUnityGameObject = @"MSPUnityListener";
static NSString *const kOnInitMethod = @"OnNativeInit";
static NSString *const kOnLoadMethod = @"OnNativeLoad";
static NSString *const kOnEventMethod = @"OnNativeEvent";
static NSString *const kOnErrorMethod = @"OnNativeError";

static NSMutableDictionary<NSString *, MSPAdLoader *> *g_adLoaders;
static NSMutableDictionary<NSString *, MSPAd *> *g_loadedAds;

static void SendUnityMessage(NSString *method, NSDictionary *payload) {
    if (method == nil || payload == nil) {
        return;
    }
    NSError *error = nil;
    NSData *data = [NSJSONSerialization dataWithJSONObject:payload options:0 error:&error];
    if (error != nil || data == nil) {
        return;
    }
    NSString *json = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    UnitySendMessage(kUnityGameObject.UTF8String, method.UTF8String, json.UTF8String);
}

@interface MSPUnityIOSAdListener : NSObject <AdListener>
@property(nonatomic, copy) NSString *placementId;
@property(nonatomic, copy) NSString *requestToken;
@end

@implementation MSPUnityIOSAdListener
- (void)onAdLoadedWithPlacementId:(NSString * _Nonnull)placementId loadInfo:(NSDictionary<NSString *,id> * _Nonnull)loadInfo {
    NSDictionary *payload = @{
        @"placementId": placementId ?: @"",
        @"requestToken": self.requestToken ?: @"",
        @"loadInfo": loadInfo ?: @{}
    };
    SendUnityMessage(kOnLoadMethod, payload);
}

- (void)onErrorWithMsg:(NSString * _Nonnull)msg loadInfo:(NSDictionary<NSString *,id> * _Nonnull)loadInfo {
    NSDictionary *payload = @{
        @"placementId": self.placementId ?: @"",
        @"requestToken": self.requestToken ?: @"",
        @"error": msg ?: @"unknown",
        @"loadInfo": loadInfo ?: @{}
    };
    SendUnityMessage(kOnErrorMethod, payload);
}

- (void)onAdImpressionWithAd:(MSPAd * _Nonnull)ad {
    SendUnityMessage(kOnEventMethod, @{
        @"placementId": self.placementId ?: @"",
        @"requestToken": self.requestToken ?: @"",
        @"event": @"impression"
    });
}

- (void)onAdClickWithAd:(MSPAd * _Nonnull)ad {
    SendUnityMessage(kOnEventMethod, @{
        @"placementId": self.placementId ?: @"",
        @"requestToken": self.requestToken ?: @"",
        @"event": @"clicked"
    });
}

- (void)onAdDismissedWithAd:(MSPAd * _Nonnull)ad {
    SendUnityMessage(kOnEventMethod, @{
        @"placementId": self.placementId ?: @"",
        @"requestToken": self.requestToken ?: @"",
        @"event": @"dismissed"
    });
}

- (UIViewController * _Nullable)getRootViewController {
    UIWindow *keyWindow = UIApplication.sharedApplication.connectedScenes.allObjects.firstObject.delegate.window;
    return keyWindow.rootViewController;
}
@end

extern "C" const char* msp_unity_get_version() {
    NSString *version = MSP.shared.version ?: @"";
    return strdup(version.UTF8String);
}

extern "C" void msp_unity_initialize(const char* prebidApiKey, int orgId, int appId, bool isInTestMode) {
    (void)isInTestMode;
    if (g_adLoaders == nil) {
        g_adLoaders = [NSMutableDictionary dictionary];
    }
    if (g_loadedAds == nil) {
        g_loadedAds = [NSMutableDictionary dictionary];
    }

    NSString *apiKey = prebidApiKey ? [NSString stringWithUTF8String:prebidApiKey] : @"";
    InitializationParametersImp *params = [[InitializationParametersImp alloc]
        initWithPrebidAPIKey:apiKey
        orgId:orgId
        appId:appId];
    [MSP.shared initMSPWithInitParams:params sdkInitListener:nil adNetworkManagers:@[]];

    SendUnityMessage(kOnInitMethod, @{@"status": @"initialized", @"message": @"MSP iOS init called"});
}

extern "C" void msp_unity_load_ad(const char* placementId, const char* requestToken) {
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    NSString *token = requestToken ? [NSString stringWithUTF8String:requestToken] : @"";

    MSPAdLoader *loader = [[MSPAdLoader alloc] init];
    MSPUnityIOSAdListener *listener = [[MSPUnityIOSAdListener alloc] init];
    listener.placementId = pid;
    listener.requestToken = token;

    AdRequest *request = [[AdRequest alloc]
        initWithCustomParams:@{}
        geo:nil
        context:nil
        adaptiveBannerSize:nil
        adSize:nil
        placementId:pid
        adFormat:AdFormatInterstitial
        testParams:@{}];
    [loader loadAdWithPlacementId:pid adListener:listener adRequest:request];

    MSPAd *ad = [loader getAdWithPlacementId:pid];
    if (ad != nil) {
        g_loadedAds[token] = ad;
    }
    g_adLoaders[token] = loader;
}

extern "C" bool msp_unity_get_ad(const char* placementId, const char* requestToken) {
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    NSString *token = requestToken ? [NSString stringWithUTF8String:requestToken] : @"";
    MSPAd *ad = g_loadedAds[token];
    (void)pid;
    return [ad isKindOfClass:[InterstitialAd class]];
}

extern "C" void msp_unity_show_ad(const char* placementId, const char* requestToken) {
    NSString *pid = placementId ? [NSString stringWithUTF8String:placementId] : @"";
    NSString *token = requestToken ? [NSString stringWithUTF8String:requestToken] : @"";
    MSPAd *ad = g_loadedAds[token];
    if ([ad isKindOfClass:[InterstitialAd class]]) {
        InterstitialAd *interstitialAd = (InterstitialAd *)ad;
        [interstitialAd show];
    } else {
        SendUnityMessage(kOnErrorMethod, @{
            @"placementId": pid ?: @"",
            @"requestToken": token ?: @"",
            @"error": @"Interstitial ad is not available."
        });
    }
}
