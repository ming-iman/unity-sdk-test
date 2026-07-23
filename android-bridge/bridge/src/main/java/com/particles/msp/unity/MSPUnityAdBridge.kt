package com.particles.msp.unity

import android.content.Context
import android.util.Log
import com.particles.msp.api.AdFormat
import com.particles.msp.api.AdListener
import com.particles.msp.api.AdLoader
import com.particles.msp.api.AdRequest
import com.particles.msp.api.InterstitialAd
import com.particles.msp.api.MSPAd
import com.unity3d.player.UnityPlayer
import java.util.UUID
import java.util.concurrent.ConcurrentHashMap

internal class MSPUnityAdBridge {
    private val adLoaders = ConcurrentHashMap<String, AdLoader>()
    private val adListeners = ConcurrentHashMap<String, UnityAdListener>()
    private val readyAds = ConcurrentHashMap<String, MSPAd>()
    private val placementsByLoader = ConcurrentHashMap<String, String>()

    fun createAdLoader(): String {
        val loaderId = UUID.randomUUID().toString().replace("-", "")
        adLoaders[loaderId] = AdLoader()
        Log.i(MSPUnityBridgeConstants.TAG, "createAdLoader loaderId=$loaderId")
        return loaderId
    }

    fun destroyAdLoader(loaderId: String) {
        adLoaders.remove(loaderId)
        adListeners.remove(loaderId)
        readyAds.remove(loaderId)
        placementsByLoader.remove(loaderId)
        Log.i(MSPUnityBridgeConstants.TAG, "destroyAdLoader loaderId=$loaderId")
    }

    fun loadAd(loaderId: String, placementId: String, customParamsJson: String?, testParamsJson: String?) {
        val context = currentContext() ?: return
        val adLoader = adLoaders[loaderId]
        if (adLoader == null) {
            Log.e(MSPUnityBridgeConstants.TAG, "loadAd aborted: unknown loaderId=$loaderId")
            MSPUnityMessages.sendError(placementId, loaderId, "Unknown MSPAdLoader")
            return
        }
        val resolvedPlacementId = if (placementId.isBlank()) {
            MSPUnityBridgeConstants.DEMO_INTERSTITIAL_PLACEMENT
        } else {
            placementId
        }
        val customParams = MSPUnityJson.jsonToMap(customParamsJson)
        val testParams = MSPUnityJson.jsonToMap(testParamsJson)
        Log.i(
            MSPUnityBridgeConstants.TAG,
            "loadAd called. loaderId=$loaderId placementId=$resolvedPlacementId customParams=$customParams testParams=$testParams"
        )
        readyAds.remove(loaderId)
        placementsByLoader[loaderId] = resolvedPlacementId
        val adListener = UnityAdListener(resolvedPlacementId, loaderId)
        adListeners[loaderId] = adListener

        val adRequest = AdRequest.Builder(AdFormat.INTERSTITIAL)
            .setContext(context)
            .setPlacement(resolvedPlacementId)
            .setCustomParams(customParams)
            .setTestParams(testParams)
            .build()

        adLoader.loadAd(resolvedPlacementId, adListener, adRequest)
    }

    fun getAd(loaderId: String, placementId: String): Boolean {
        readyAds[loaderId]?.let { return it is InterstitialAd }
        val adLoader = adLoaders[loaderId] ?: return false
        val ad = adLoader.getAd(placementId) ?: return false
        if (ad !is InterstitialAd) {
            return false
        }
        readyAds[loaderId] = ad
        return true
    }

    fun showAd(loaderId: String) {
        val activity = UnityPlayer.currentActivity
        val placementId = placementsByLoader[loaderId].orEmpty()
        if (activity == null) {
            MSPUnityMessages.sendError(placementId, loaderId, "Unity activity is null.")
            return
        }
        val ad = readyAds[loaderId]
        if (ad !is InterstitialAd) {
            Log.e(
                MSPUnityBridgeConstants.TAG,
                "showAd failed. loaderId=$loaderId placementId=$placementId cachedAd=${ad?.javaClass?.simpleName}"
            )
            MSPUnityMessages.sendError(placementId, loaderId, "Interstitial ad is not available.")
            return
        }
        activity.runOnUiThread {
            Log.i(MSPUnityBridgeConstants.TAG, "showAd loaderId=$loaderId placementId=$placementId")
            ad.show(activity)
        }
    }

    private fun currentContext(): Context? = UnityPlayer.currentActivity?.applicationContext

    private inner class UnityAdListener(
        private val placementId: String,
        private val loaderId: String,
    ) : AdListener {
        override fun onAdLoaded(placementId: String, loadInfo: Map<String, Any>) {
            Log.i(MSPUnityBridgeConstants.TAG, "onAdLoaded placementId=$placementId loadInfo=$loadInfo")
            val adLoader = adLoaders[loaderId]
            val ad = adLoader?.getAd(placementId)
            if (ad != null) {
                readyAds[loaderId] = ad
                Log.i(MSPUnityBridgeConstants.TAG, "onAdLoaded cached ad type=${ad.javaClass.simpleName}")
            } else {
                Log.e(MSPUnityBridgeConstants.TAG, "onAdLoaded failed to cache ad for loaderId=$loaderId")
            }
            MSPUnityMessages.sendLoad(placementId, loaderId, loadInfo)
        }

        override fun onError(msg: String, loadInfo: Map<String, Any>) {
            Log.e(MSPUnityBridgeConstants.TAG, "onError placementId=$placementId msg=$msg loadInfo=$loadInfo")
            MSPUnityMessages.sendError(placementId, loaderId, msg, loadInfo)
        }

        override fun onAdClicked(ad: MSPAd) {
            MSPUnityMessages.sendLifecycle(placementId, loaderId, "clicked")
        }

        override fun onAdImpression(ad: MSPAd) {
            MSPUnityMessages.sendLifecycle(placementId, loaderId, "impression")
        }

        override fun onAdDismissed(ad: MSPAd) {
            MSPUnityMessages.sendLifecycle(placementId, loaderId, "dismissed")
        }
    }
}
