#import <Foundation/Foundation.h>
#import <TargetConditionals.h>
#import <FBAudienceNetwork/FBAdSettings.h>

// Registers this device for Facebook Audience Network test ads.
// FAN prints the device hash in Xcode when test mode is on; update if you switch devices.
__attribute__((constructor))
static void GridLightRegisterFacebookTestDevice(void)
{
#if !TARGET_OS_SIMULATOR
    [FBAdSettings addTestDevice:@"abb6200e1509579fa04de6fb82615e85dd41f6c0"];
#endif
    if (@available(iOS 14.0, *)) {
        [FBAdSettings setAdvertiserTrackingEnabled:YES];
    }
}
