import Foundation
import MSPCore
import MSPiOSCore
import UIKit

final class UnityInitListener: NSObject, MSPInitListener {
    func onComplete(status: MSPInitStatus, message: String) {
        MSPUnityEntry.sendUnityMessage(
            method: MSPUnityEntry.onInitMethod,
            payload: ["status": "SUCCESS", "message": message]
        )
        MSPUnityEntry.initListener = nil
    }
}

final class UnityAdListener: NSObject, AdListener {
    let loaderId: String
    let placementId: String
    weak var loader: MSPAdLoader?

    init(loaderId: String, placementId: String, loader: MSPAdLoader) {
        self.loaderId = loaderId
        self.placementId = placementId
        self.loader = loader
        super.init()
    }

    func onAdLoaded(placementId _: String) {}

    func onAdLoaded(placementId: String, loadInfo _: [String: Any]) {
        if let ad = loader?.getAd(placementId: placementId) {
            MSPUnityEntry.setLoadedAd(ad, for: loaderId)
        }
        MSPUnityEntry.sendUnityMessage(
            method: MSPUnityEntry.onLoadMethod,
            payload: ["loaderId": loaderId, "placementId": placementId]
        )
    }

    func onAdImpression(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: MSPUnityEntry.onEventMethod,
            payload: [
                "event": "display",
                "loaderId": loaderId,
                "placementId": placementId,
            ]
        )
    }

    func onAdDismissed(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: MSPUnityEntry.onEventMethod,
            payload: [
                "event": "hide",
                "loaderId": loaderId,
                "placementId": placementId,
            ]
        )
        MSPUnityEntry.clearAdState(for: loaderId)
    }

    func onAdClick(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: MSPUnityEntry.onEventMethod,
            payload: [
                "event": "click",
                "loaderId": loaderId,
                "placementId": placementId,
            ]
        )
    }

    func onError(msg: String) {
        onError(msg: msg, loadInfo: [:])
    }

    func onError(msg: String, loadInfo _: [String: Any]) {
        MSPUnityEntry.sendUnityMessage(
            method: MSPUnityEntry.onErrorMethod,
            payload: [
                "error": msg,
                "loaderId": loaderId,
                "placementId": placementId,
            ]
        )
        MSPUnityEntry.clearAdState(for: loaderId)
    }

    func onAdRewardReceived(ad _: MSPAd) {
        MSPUnityEntry.sendUnityMessage(
            method: MSPUnityEntry.onEventMethod,
            payload: [
                "event": "reward",
                "loaderId": loaderId,
                "placementId": placementId,
            ]
        )
    }

    func getRootViewController() -> UIViewController? {
        MSPUnityEntry.topViewController()
    }
}
