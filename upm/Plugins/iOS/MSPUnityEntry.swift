import Darwin
import Foundation
import MSPCore
import MSPiOSCore
import ObjectiveC
import PrebidMobile
import UIKit

@_silgen_name("UnitySendMessage")
private func UnitySendMessage(_ obj: UnsafePointer<CChar>, _ method: UnsafePointer<CChar>, _ msg: UnsafePointer<CChar>)

private final class UnityInitializationParameters: InitializationParameters {
    let prebidAPIKey: String
    let sourceApp: String?
    let prebidHostUrl: String
    let parameters: [String: Any]
    let ageRestrictedUser: Bool
    let testMode: Bool

    init(
        prebidAPIKey: String,
        sourceApp: String?,
        prebidHostUrl: String,
        parameters: [String: Any],
        ageRestrictedUser: Bool,
        testMode: Bool
    ) {
        self.prebidAPIKey = prebidAPIKey
        self.sourceApp = sourceApp
        self.prebidHostUrl = prebidHostUrl
        self.parameters = parameters
        self.ageRestrictedUser = ageRestrictedUser
        self.testMode = testMode
    }

    func getPrebidAPIKey() -> String { prebidAPIKey }
    func getPrebidHostUrl() -> String { prebidHostUrl }
    func getAppStoreId() -> String? { sourceApp }
    func getParameters() -> [String: Any]? { parameters }
    func isAgeRestrictedUser() -> Bool { ageRestrictedUser }
    func isInTestMode() -> Bool { testMode }
}

private struct UnityInitializationConfig {
    let prebidAPIKey: String
    let sourceApp: String
    let orgId: Int64
    let appId: Int64
    let prebidHost: String
    let isAgeRestrictedUser: Bool
    let isInTestMode: Bool
    let parameters: [String: Any]

    var resolvedPrebidHostUrl: String {
        guard !prebidHost.isEmpty else {
            return MSP.shared.prebidHost + "/openrtb2/auction"
        }
        let normalized = prebidHost.trimmingCharacters(in: CharacterSet(charactersIn: "/"))
        return normalized.hasSuffix("/openrtb2/auction")
            ? normalized
            : normalized + "/openrtb2/auction"
    }

    var resolvedPrebidHostBase: String? {
        guard !prebidHost.isEmpty else {
            return nil
        }
        let normalized = prebidHost.trimmingCharacters(in: CharacterSet(charactersIn: "/"))
        return normalized.hasSuffix("/openrtb2/auction")
            ? String(normalized.dropLast("/openrtb2/auction".count))
            : normalized
    }
}

@objc(MSPUnityEntry)
@objcMembers
public final class MSPUnityEntry: NSObject {
    private static let unityGameObject = "MSPUnityListener"
    private static let onInitMethod = "OnNativeInit"
    private static let onLoadMethod = "OnNativeLoad"
    private static let onEventMethod = "OnNativeEvent"
    private static let onErrorMethod = "OnNativeError"

    fileprivate static var adLoaders: [String: MSPAdLoader] = [:]
    fileprivate static var loadedAds: [String: MSPAd] = [:]
    fileprivate static var adListeners: [String: UnityAdListener] = [:]
    private static var registeredManagers: [AdNetworkManager] = []
    private static let linkedOptionalAdapterIds = [
        "nova",
        "google",
        "facebook",
        "unity",
        "inmobi",
        "mobilefuse",
        "mintegral",
        "pubmatic",
        "moloco",
        "amazon",
        "liftoff",
        "applovin"
    ]

    public static func registerManager(_ manager: AdNetworkManager) {
        registeredManagers.append(manager)
    }

    private static func activateLinkedOptionalAdaptersIfNeeded() {
        guard registeredManagers.isEmpty else {
            return
        }

        for adapterId in linkedOptionalAdapterIds {
            if activateAdapterViaCdecl(adapterId: adapterId) {
                MSPLogger.shared.info(message: "[MSPUnity] Linked optional adapter '\(adapterId)' activated during init")
            }
        }
    }

    @discardableResult
    public static func activateAdapter(adapterId: String, bootstrapClassName: String) -> Bool {
        if activateAdapterViaCdecl(adapterId: adapterId) {
            MSPLogger.shared.info(message: "[MSPUnity] Activated adapter '\(adapterId)' via native symbol")
            return true
        }

        if activateAdapterBootstrap(bootstrapClassName) {
            MSPLogger.shared.info(message: "[MSPUnity] Activated adapter '\(adapterId)' via bootstrap class")
            return true
        }

        MSPLogger.shared.fault(message: "[MSPUnity] Failed to activate adapter '\(adapterId)'")
        return false
    }

    @discardableResult
    private static func activateAdapterViaCdecl(adapterId: String) -> Bool {
        guard !adapterId.isEmpty else {
            return false
        }

        let symbol = "msp_unity_register_adapter_\(adapterId)"
        guard let sym = dlsym(UnsafeMutableRawPointer(bitPattern: -2), symbol) else {
            return false
        }

        typealias RegisterFn = @convention(c) () -> Void
        unsafeBitCast(sym, to: RegisterFn.self)()
        return true
    }

    @discardableResult
    private static func activateAdapterBootstrap(_ bootstrapClassName: String) -> Bool {
        guard !bootstrapClassName.isEmpty,
              let cls = NSClassFromString(bootstrapClassName) as? NSObject.Type
        else {
            return false
        }

        let selector = NSSelectorFromString("activate")
        guard cls.responds(to: selector),
              let method = class_getClassMethod(cls, selector)
        else {
            return false
        }

        typealias ActivateIMP = @convention(c) (AnyClass, Selector) -> Void
        let imp = method_getImplementation(method)
        unsafeBitCast(imp, to: ActivateIMP.self)(cls, selector)
        return true
    }

    public static func sdkVersion() -> String {
        MSP.shared.version
    }

    public static func setLogLevel(_ level: Int32) {
        let mappedLevel: Int
        switch Int(level) {
        case ..<MSPLogLevel.DEBUG:
            mappedLevel = MSPLogger.DEBUG
        case MSPLogLevel.DEBUG:
            mappedLevel = MSPLogger.DEBUG
        case MSPLogLevel.INFO:
            mappedLevel = MSPLogger.INFO
        case MSPLogLevel.WARN:
            mappedLevel = MSPLogger.ERROR
        case MSPLogLevel.ERROR:
            mappedLevel = MSPLogger.FAULT
        case MSPLogLevel.ASSERT:
            mappedLevel = MSPLogger.FAULT
        case Int.max:
            mappedLevel = MSPLogger.NONE
        default:
            mappedLevel = MSPLogger.INFO
        }
        MSPLogger.shared.setLogLevel(level: mappedLevel)
    }

    @discardableResult
    public static func initialize(prebidApiKey: String, orgId: Int32, appId: Int32, isInTestMode: Bool) -> Bool {
        initialize(
            config: UnityInitializationConfig(
                prebidAPIKey: prebidApiKey,
                sourceApp: "",
                orgId: Int64(orgId),
                appId: Int64(appId),
                prebidHost: "",
                isAgeRestrictedUser: false,
                isInTestMode: isInTestMode,
                parameters: [:]
            )
        )
    }

    @discardableResult
    public static func initialize(json: String) -> Bool {
        guard let data = json.data(using: .utf8),
              let object = try? JSONSerialization.jsonObject(with: data),
              let dictionary = object as? [String: Any]
        else {
            sendUnityMessage(
                method: onInitMethod,
                payload: ["status": "FAILURE", "message": "Invalid MSP initialization JSON"]
            )
            return false
        }

        let config = UnityInitializationConfig(
            prebidAPIKey: dictionary["prebidApiKey"] as? String ?? "",
            sourceApp: dictionary["sourceApp"] as? String ?? "",
            orgId: (dictionary["orgId"] as? NSNumber)?.int64Value ?? 0,
            appId: (dictionary["appId"] as? NSNumber)?.int64Value ?? 0,
            prebidHost: dictionary["prebidHost"] as? String ?? "",
            isAgeRestrictedUser: dictionary["isAgeRestrictedUser"] as? Bool ?? false,
            isInTestMode: dictionary["isInTestMode"] as? Bool ?? false,
            parameters: dictionary["parameters"] as? [String: Any] ?? [:]
        )
        return initialize(config: config)
    }

    private static func initialize(config: UnityInitializationConfig) -> Bool {
        if let prebidHostBase = config.resolvedPrebidHostBase {
            MSP.shared.prebidHost = prebidHostBase
        }
        MSP.shared.orgId = config.orgId
        MSP.shared.appId = config.appId
        MSP.shared.org = String(config.orgId)
        MSP.shared.app = String(config.appId)
        MSP.shared.prebidAPIKey = config.prebidAPIKey
        MSPUserAgent.provider = MSP.shared
        if !config.sourceApp.isEmpty {
            // MSP iOS 4.5.0 only copies sourceApp from InitializationParametersImp.
            // This bridge uses a protocol implementation so test/privacy flags can also flow.
            Targeting.shared.sourceapp = config.sourceApp
        }

        activateLinkedOptionalAdaptersIfNeeded()
        let managers = registeredManagers
        MSPLogger.shared.info(message: "[MSPUnity] Initializing MSP with \(managers.count) ad network manager(s)")
        let unityParams = UnityInitializationParameters(
            prebidAPIKey: config.prebidAPIKey,
            sourceApp: config.sourceApp.isEmpty ? nil : config.sourceApp,
            prebidHostUrl: config.resolvedPrebidHostUrl,
            parameters: config.parameters,
            ageRestrictedUser: config.isAgeRestrictedUser,
            testMode: config.isInTestMode
        )
        MSP.shared.initMSP(initParams: unityParams, sdkInitListener: nil, adNetworkManagers: managers)

        sendUnityMessage(
            method: onInitMethod,
            payload: ["status": "SUCCESS", "message": "MSP iOS init called"]
        )
        return true
    }

    @discardableResult
    public static func loadAd(
        placementId: String,
        requestToken: String,
        customParamsJson: String?,
        testParamsJson: String?
    ) -> Bool {
        guard !placementId.isEmpty, !requestToken.isEmpty else {
            sendUnityMessage(
                method: onErrorMethod,
                payload: ["error": "placementId/requestToken is empty", "placementId": placementId, "requestToken": requestToken]
            )
            return false
        }

        let loader = MSPAdLoader()
        let listener = UnityAdListener(
            requestToken: requestToken,
            placementId: placementId,
            loader: loader
        )
        adListeners[requestToken] = listener
        let customParams = parseJsonObject(customParamsJson)
        let testParams = parseJsonObject(testParamsJson)
        let request = AdRequest(
            customParams: customParams,
            geo: nil,
            context: nil,
            adaptiveBannerSize: nil,
            adSize: nil,
            placementId: placementId,
            adFormat: .interstitial,
            testParams: testParams
        )

        adLoaders[requestToken] = loader
        loader.loadAd(
            placementId: placementId,
            adListener: listener,
            adRequest: request
        )
        return true
    }

    private static func parseJsonObject(_ json: String?) -> [String: Any] {
        guard let json = json?.trimmingCharacters(in: .whitespacesAndNewlines),
              !json.isEmpty,
              let data = json.data(using: .utf8),
              let object = try? JSONSerialization.jsonObject(with: data) as? [String: Any]
        else {
            return [:]
        }
        return object
    }

    public static func hasAd(placementId _: String, requestToken: String) -> Bool {
        guard let ad = loadedAds[requestToken] else { return false }
        return ad is InterstitialAd
    }

    @discardableResult
    public static func showAd(placementId: String, requestToken: String) -> Bool {
        guard let root = topViewController() else {
            sendUnityMessage(method: onErrorMethod, payload: [
                "error": "No rootViewController",
                "placementId": placementId,
                "requestToken": requestToken,
            ])
            return false
        }

        guard let ad = loadedAds[requestToken] as? InterstitialAd else {
            sendUnityMessage(method: onErrorMethod, payload: [
                "error": "Ad not available",
                "placementId": placementId,
                "requestToken": requestToken,
            ])
            return false
        }

        Task { @MainActor in
            ad.show(rootViewController: root)
        }
        return true
    }

    fileprivate static func setLoadedAd(_ ad: MSPAd, for token: String) {
        loadedAds[token] = ad
    }

    fileprivate static func clearAdState(for token: String) {
        loadedAds.removeValue(forKey: token)
        adLoaders.removeValue(forKey: token)
        adListeners.removeValue(forKey: token)
    }

    fileprivate static func sendUnityMessage(method: String, payload: [String: Any]) {
        guard JSONSerialization.isValidJSONObject(payload),
              let data = try? JSONSerialization.data(withJSONObject: payload, options: []),
              let json = String(data: data, encoding: .utf8)
        else {
            return
        }

        unityGameObject.withCString { goPtr in
            method.withCString { methodPtr in
                json.withCString { msgPtr in
                    UnitySendMessage(goPtr, methodPtr, msgPtr)
                }
            }
        }
    }

    fileprivate static func topViewController() -> UIViewController? {
        let scene = UIApplication.shared.connectedScenes
            .compactMap { $0 as? UIWindowScene }
            .first { $0.activationState == .foregroundActive }

        let window = scene?.windows.first(where: \.isKeyWindow)
            ?? UIApplication.shared.windows.first(where: \.isKeyWindow)
            ?? UIApplication.shared.windows.first

        var root = window?.rootViewController
        while let presented = root?.presentedViewController {
            root = presented
        }
        return root
    }
}

private enum MSPLogLevel {
    static let VERBOSE = 2
    static let DEBUG = 3
    static let INFO = 4
    static let WARN = 5
    static let ERROR = 6
    static let ASSERT = 7
}

private final class UnityAdListener: NSObject, AdListener {
    let requestToken: String
    let placementId: String
    weak var loader: MSPAdLoader?

    init(requestToken: String, placementId: String, loader: MSPAdLoader) {
        self.requestToken = requestToken
        self.placementId = placementId
        self.loader = loader
        super.init()
    }

    func onAdLoaded(placementId _: String) {}

    func onAdLoaded(placementId: String, loadInfo _: [String: Any]) {
        if let ad = loader?.getAd(placementId: placementId) {
            MSPUnityEntry.setLoadedAd(ad, for: requestToken)
        }
        MSPUnityEntry.sendUnityMessage(
            method: "OnNativeLoad",
            payload: ["requestToken": requestToken, "placementId": placementId]
        )
    }

    func onAdImpression(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: "OnNativeEvent",
            payload: [
                "event": "display",
                "requestToken": requestToken,
                "placementId": placementId,
            ]
        )
    }

    func onAdDismissed(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: "OnNativeEvent",
            payload: [
                "event": "hide",
                "requestToken": requestToken,
                "placementId": placementId,
            ]
        )
        MSPUnityEntry.clearAdState(for: requestToken)
    }

    func onAdClick(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: "OnNativeEvent",
            payload: [
                "event": "click",
                "requestToken": requestToken,
                "placementId": placementId,
            ]
        )
    }

    func onError(msg: String) {
        onError(msg: msg, loadInfo: [:])
    }

    func onError(msg: String, loadInfo _: [String: Any]) {
        MSPUnityEntry.sendUnityMessage(
            method: "OnNativeError",
            payload: [
                "error": msg,
                "requestToken": requestToken,
                "placementId": placementId,
            ]
        )
        MSPUnityEntry.clearAdState(for: requestToken)
    }

    func onAdRewardReceived(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: "OnNativeEvent",
            payload: [
                "event": "reward",
                "requestToken": requestToken,
                "placementId": placementId,
            ]
        )
    }

    func getRootViewController() -> UIViewController? {
        MSPUnityEntry.topViewController()
    }

}
