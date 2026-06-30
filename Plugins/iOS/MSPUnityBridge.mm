#import <Foundation/Foundation.h>

extern "C" const char* msp_unity_get_version() {
    return "ios-bridge-skeleton";
}

extern "C" void msp_unity_initialize(const char* prebidApiKey, int orgId, int appId, bool isInTestMode) {
    // TODO: wire to MSP.shared.initMSP(...) using wrapper params.
    (void)prebidApiKey;
    (void)orgId;
    (void)appId;
    (void)isInTestMode;
}

extern "C" void msp_unity_load_interstitial(const char* placementId, const char* requestToken) {
    // TODO: wire to MSPAdLoader.loadAd(...)
    (void)placementId;
    (void)requestToken;
}

extern "C" void msp_unity_show_interstitial(const char* placementId, const char* requestToken) {
    // TODO: wire to MSPInterstitialAd.show(...)
    (void)placementId;
    (void)requestToken;
}
