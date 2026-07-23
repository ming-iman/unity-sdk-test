import Foundation
import MSPCore
import MSPiOSCore
import UIKit

@_silgen_name("UnitySendMessage")
private func UnitySendMessage(_ obj: UnsafePointer<CChar>, _ method: UnsafePointer<CChar>, _ msg: UnsafePointer<CChar>)

extension MSPUnityEntry {
    static let unityGameObject = "MSPUnityListener"
    static let onInitMethod = "OnNativeInit"
    static let onLoadMethod = "OnNativeLoad"
    static let onEventMethod = "OnNativeEvent"
    static let onErrorMethod = "OnNativeError"

    static var adLoaders: [String: MSPAdLoader] = [:]
    static var loadedAds: [String: MSPAd] = [:]
    static var adListeners: [String: UnityAdListener] = [:]
    static var placementsByLoader: [String: String] = [:]
    static var registeredManagers: [AdNetworkManager] = []
    static var initListener: UnityInitListener?
    static let linkedOptionalAdapterIds = [
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

    static func setLoadedAd(_ ad: MSPAd, for loaderId: String) {
        loadedAds[loaderId] = ad
    }

    static func clearAdState(for loaderId: String) {
        loadedAds.removeValue(forKey: loaderId)
        adListeners.removeValue(forKey: loaderId)
        // Keep the native MSPAdLoader alive until destroyAdLoader so the C# instance can reload.
    }

    static func sendUnityMessage(method: String, payload: [String: Any]) {
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

    static func topViewController() -> UIViewController? {
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
