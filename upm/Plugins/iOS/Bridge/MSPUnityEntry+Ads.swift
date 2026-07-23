import Foundation
import MSPCore
import MSPiOSCore

extension MSPUnityEntry {
    @objc
    public static func createAdLoader() -> String {
        let loaderId = UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased()
        adLoaders[loaderId] = MSPAdLoader()
        MSPLogger.shared.info(message: "[MSPUnity] createAdLoader loaderId=\(loaderId)")
        return loaderId
    }

    @objc(destroyAdLoaderWithLoaderId:)
    public static func destroyAdLoader(loaderId: String) {
        loadedAds.removeValue(forKey: loaderId)
        adLoaders.removeValue(forKey: loaderId)
        adListeners.removeValue(forKey: loaderId)
        placementsByLoader.removeValue(forKey: loaderId)
        MSPLogger.shared.info(message: "[MSPUnity] destroyAdLoader loaderId=\(loaderId)")
    }

    @objc(loadAdWithLoaderId:placementId:customParamsJson:testParamsJson:)
    @discardableResult
    public static func loadAd(
        loaderId: String,
        placementId: String,
        customParamsJson: String?,
        testParamsJson: String?
    ) -> Bool {
        guard !loaderId.isEmpty, !placementId.isEmpty else {
            sendUnityMessage(
                method: onErrorMethod,
                payload: ["error": "loaderId/placementId is empty", "placementId": placementId, "loaderId": loaderId]
            )
            return false
        }

        guard let loader = adLoaders[loaderId] else {
            sendUnityMessage(
                method: onErrorMethod,
                payload: ["error": "Unknown MSPAdLoader", "placementId": placementId, "loaderId": loaderId]
            )
            return false
        }

        loadedAds.removeValue(forKey: loaderId)
        placementsByLoader[loaderId] = placementId
        let listener = UnityAdListener(loaderId: loaderId, placementId: placementId, loader: loader)
        adListeners[loaderId] = listener
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

        loader.loadAd(placementId: placementId, adListener: listener, adRequest: request)
        return true
    }

    @objc(hasAdWithLoaderId:placementId:)
    public static func hasAd(loaderId: String, placementId _: String) -> Bool {
        guard let ad = loadedAds[loaderId] else { return false }
        return ad is InterstitialAd
    }

    @objc(showAdWithLoaderId:)
    @discardableResult
    public static func showAd(loaderId: String) -> Bool {
        let placementId = placementsByLoader[loaderId] ?? ""
        guard let root = topViewController() else {
            sendUnityMessage(method: onErrorMethod, payload: [
                "error": "No rootViewController",
                "placementId": placementId,
                "loaderId": loaderId,
            ])
            return false
        }

        guard let ad = loadedAds[loaderId] as? InterstitialAd else {
            sendUnityMessage(method: onErrorMethod, payload: [
                "error": "Ad not available",
                "placementId": placementId,
                "loaderId": loaderId,
            ])
            return false
        }

        Task { @MainActor in
            ad.show(rootViewController: root)
        }
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
}
