import Darwin
import Foundation
import MSPCore
import MSPiOSCore
import ObjectiveC
import PrebidMobile

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

private enum MSPLogLevel {
    static let DEBUG = 3
    static let INFO = 4
    static let WARN = 5
    static let ERROR = 6
    static let ASSERT = 7
}

extension MSPUnityEntry {
    @objc
    public static func registerManager(_ manager: AdNetworkManager) {
        registeredManagers.append(manager)
    }

    @objc
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

    @objc
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

    @objc
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

    @objc(initializeWithJson:)
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
        let listener = UnityInitListener()
        initListener = listener
        MSP.shared.initMSP(initParams: unityParams, sdkInitListener: listener, adNetworkManagers: managers)
        return true
    }
}
