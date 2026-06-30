package com.particles.msp.unity

import android.content.Context
import com.particles.msp.api.AdFormat
import com.particles.msp.api.AdLoader
import com.particles.msp.api.AdRequest
import com.particles.msp.api.AdListener
import com.particles.msp.api.InterstitialAd
import com.particles.msp.api.MSPAd
import com.particles.msp.api.MSPInitListener
import com.particles.msp.api.MSPInitStatus
import com.particles.msp.api.MSPInitializationParameters
import com.particles.prebidadapter.MSP
import com.unity3d.player.UnityPlayer
import org.json.JSONObject
import java.util.concurrent.ConcurrentHashMap

object MSPUnityBridge {
    private const val UNITY_GAME_OBJECT = "MSPUnityListener"
    private const val ON_INIT = "OnNativeInit"
    private const val ON_LOAD = "OnNativeLoad"
    private const val ON_EVENT = "OnNativeEvent"
    private const val ON_ERROR = "OnNativeError"

    private val adLoaders = ConcurrentHashMap<String, AdLoader>()
    private val adListeners = ConcurrentHashMap<String, UnityAdListener>()

    @JvmStatic
    fun getVersion(): String = MSP.version

    @JvmStatic
    fun initialize(prebidApiKey: String, orgId: Int, appId: Int, isInTestMode: Boolean) {
        val context = currentContext() ?: return
        val initParams = object : MSPInitializationParameters {
            override fun getPrebidAPIKey(): String = prebidApiKey
            override fun getOrgId(): Int = orgId
            override fun getAppId(): Int = appId
            override fun getConsentString(): String = ""
            override fun getParameters(): Map<String, Any> = emptyMap()
            override fun hasUserConsent(): Boolean = true
            override fun isAgeRestrictedUser(): Boolean = false
            override fun isDoNotSell(): Boolean = false
            override fun isInTestMode(): Boolean = isInTestMode
        }
        MSP.init(context, initParams, object : MSPInitListener {
            override fun onComplete(status: MSPInitStatus, message: String) {
                val payload = JSONObject()
                    .put("status", status.name)
                    .put("message", message)
                    .toString()
                UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_INIT, payload)
            }
        })
    }

    @JvmStatic
    fun loadAd(placementId: String, requestToken: String) {
        val context = currentContext() ?: return
        val adLoader = AdLoader()
        val adListener = UnityAdListener(placementId, requestToken)
        adLoaders[requestToken] = adLoader
        adListeners[requestToken] = adListener

        val adRequest = AdRequest.Builder(AdFormat.interstitial)
            .setContext(context)
            .setPlacement(placementId)
            .build()

        adLoader.loadAd(placementId, adListener, adRequest)
    }

    @JvmStatic
    fun getAd(placementId: String, requestToken: String): Boolean {
        val adLoader = adLoaders[requestToken] ?: return false
        val ad = adLoader.getAd(placementId)
        return ad is InterstitialAd
    }

    @JvmStatic
    fun showAd(placementId: String, requestToken: String) {
        val activity = UnityPlayer.currentActivity ?: return
        val adLoader = adLoaders[requestToken] ?: return
        val ad = adLoader.getAd(placementId)
        if (ad is InterstitialAd) {
            ad.show(activity)
        } else {
            val payload = JSONObject()
                .put("placementId", placementId)
                .put("requestToken", requestToken)
                .put("error", "Interstitial ad is not available.")
                .toString()
            UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_ERROR, payload)
        }
    }

    private fun currentContext(): Context? = UnityPlayer.currentActivity?.applicationContext

    private class UnityAdListener(
        private val placementId: String,
        private val requestToken: String,
    ) : AdListener {
        override fun onAdLoaded(placementId: String, loadInfo: Map<String, Any>) {
            val payload = JSONObject()
                .put("placementId", placementId)
                .put("requestToken", requestToken)
                .put("loadInfo", JSONObject(loadInfo))
                .toString()
            UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_LOAD, payload)
        }

        override fun onError(msg: String, loadInfo: Map<String, Any>) {
            val payload = JSONObject()
                .put("placementId", placementId)
                .put("requestToken", requestToken)
                .put("error", msg)
                .put("loadInfo", JSONObject(loadInfo))
                .toString()
            UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_ERROR, payload)
        }

        override fun onAdClicked(ad: MSPAd) {
            sendLifecycle("clicked")
        }

        override fun onAdImpression(ad: MSPAd) {
            sendLifecycle("impression")
        }

        override fun onAdDismissed(ad: MSPAd) {
            sendLifecycle("dismissed")
        }

        private fun sendLifecycle(eventName: String) {
            val payload = JSONObject()
                .put("placementId", placementId)
                .put("requestToken", requestToken)
                .put("event", eventName)
                .toString()
            UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_EVENT, payload)
        }
    }
}
