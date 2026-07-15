package com.particles.msp.unity

import android.content.Context
import android.util.Log
import com.particles.msp.api.AdFormat
import com.particles.msp.api.AdLoader
import com.particles.msp.api.AdRequest
import com.particles.msp.api.AdListener
import com.particles.msp.api.InterstitialAd
import com.particles.msp.api.MSPAd
import com.particles.msp.api.MSPConstants
import com.particles.msp.api.MSPInitListener
import com.particles.msp.api.MSPInitStatus
import com.particles.msp.api.MSPInitializationParameters
import com.particles.msp.util.HostConfig
import com.particles.msp.util.Logger
import com.particles.prebidadapter.MSP
import com.unity3d.player.UnityPlayer
import org.json.JSONObject
import java.util.concurrent.ConcurrentHashMap

object MSPUnityBridge {
    private const val TAG = "MSPUnityBridge"
    private const val UNITY_GAME_OBJECT = "MSPUnityListener"
    private const val ON_INIT = "OnNativeInit"
    private const val ON_LOAD = "OnNativeLoad"
    private const val ON_EVENT = "OnNativeEvent"
    private const val ON_ERROR = "OnNativeError"
    private const val DEMO_INTERSTITIAL_PLACEMENT = "demo-android-interstitial"
    // Default only when caller does not provide ad_network.
    private const val DEMO_TEST_AD_NETWORK = "msp_nova"

    private val adLoaders = ConcurrentHashMap<String, AdLoader>()
    private val adListeners = ConcurrentHashMap<String, UnityAdListener>()
    private val readyAds = ConcurrentHashMap<String, MSPAd>()

    @JvmStatic
    fun getVersion(): String = MSP.version

    @JvmStatic
    fun setLogLevel(level: Int) {
        try {
            Logger.setLogLevel(level)
            Log.i(TAG, "setLogLevel called via MSP Logger. level=$level")
        } catch (t: Throwable) {
            Log.w(TAG, "setLogLevel failed. level=$level reason=${t.message}")
        }
    }

    @JvmStatic
    fun initialize(prebidApiKey: String, orgId: Int, appId: Int, isInTestMode: Boolean) {
        val activity = UnityPlayer.currentActivity
        if (activity == null) {
            Log.e(TAG, "initialize aborted: Unity activity is null")
            return
        }
        val context = activity.applicationContext
        Log.i(TAG, "initialize called. orgId=$orgId appId=$appId testMode=$isInTestMode")
        if (isInTestMode) {
            enableMesDebugLogging()
        }
        val mesInitUrl = HostConfig.getMesHostUrl(orgId).trimEnd('/') + "/v1/event/sdk_init"
        Log.i(TAG, "MES sdk_init endpoint: $mesInitUrl")

        // Keep aligned with MSP Android demo profile parameters.
        val initParamsMap = mapOf(
            MSPConstants.INIT_PARAM_KEY_PPID to "shun-test-ppid",
            MSPConstants.INIT_PARAM_KEY_EMAIL to "shun.j@shun.com",
            MSPConstants.INIT_PARAM_KEY_UNITY_APP_KEY to "207789bad",
            MSPConstants.INIT_PARAM_KEY_INMOBI_ACCOUNT_ID to "3ef8dd9e9d5b4080ad1682510980b643",
            MSPConstants.INIT_PARAM_KEY_MINTEGRAL_APP_ID to "144002",
            MSPConstants.INIT_PARAM_KEY_MINTEGRAL_APP_KEY to "7c22942b749fe6a6e361b675e96b3ee9",
            MSPConstants.INIT_PARAM_KEY_PUBMATIC_PUBLISHER_ID to "156276",
            MSPConstants.INIT_PARAM_KEY_MOLOCO_APP_KEY to "NEWSBREAK:tz5zGje2JXIAhpbZ",
            MSPConstants.INIT_PARAM_KEY_AMAZON_APP_KEY to "369701c6-f17a-4573-b695-52aae43d960c",
            MSPConstants.INIT_PARAM_KEY_LIFTOFF_APP_ID to "69437de9f9db799a8390058c",
            MSPConstants.INIT_PARAM_KEY_GOOGLE_APP_ID to "ca-app-pub-3940256099942544~3347511713",
            MSPConstants.INIT_PARAM_KEY_APPLOVIN_SDK_KEY to "6KrA5SQHFTBpGDUU4FeLIZGxGFmd1rORGfr5xlrJIMeXO8pdvuKPQO4WAfQpEZ4cXAOXoeSJJRoX0zcD4qBzak",
            MSPConstants.INIT_PREBID_BID_REQUEST_TIMEOUT_MILLIS to 50000,
        )

        val initParams = object : MSPInitializationParameters {
            override fun getPrebidAPIKey(): String = prebidApiKey
            override fun getOrgId(): Int = orgId
            override fun getAppId(): Int = appId
            override fun getConsentString(): String = ""
            override fun getParameters(): Map<String, Any> = initParamsMap
            override fun hasUserConsent(): Boolean = true
            override fun isAgeRestrictedUser(): Boolean = false
            override fun isDoNotSell(): Boolean = false
            override fun isInTestMode(): Boolean = isInTestMode
        }
        activity.runOnUiThread {
            MSP.init(context, initParams, object : MSPInitListener {
                override fun onComplete(status: MSPInitStatus, message: String) {
                    Log.i(TAG, "initialize complete. status=$status message=$message")
                    val payload = JSONObject()
                        .put("status", status.name)
                        .put("message", message)
                        .toString()
                    UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_INIT, payload)
                }
            })
        }
    }

    private fun enableMesDebugLogging() {
        try {
            val clazz = Class.forName("com.particles.mes.android.MesConfig")
            val field = clazz.getDeclaredField("debug")
            field.isAccessible = true
            field.setBoolean(null, true)
            Log.i(TAG, "MesConfig.debug enabled for test mode")
        } catch (t: Throwable) {
            Log.w(TAG, "Unable to enable MesConfig.debug: ${t.message}")
        }
    }

    @JvmStatic
    fun loadAd(placementId: String, requestToken: String, adNetwork: String?) {
        val context = currentContext() ?: return
        val resolvedPlacementId = if (placementId.isBlank()) DEMO_INTERSTITIAL_PLACEMENT else placementId
        val resolvedAdNetwork = adNetwork?.trim().takeUnless { it.isNullOrEmpty() } ?: DEMO_TEST_AD_NETWORK
        Log.i(
            TAG,
            "loadAd called. placementId=$resolvedPlacementId requestToken=$requestToken adNetwork=$resolvedAdNetwork"
        )
        readyAds.remove(requestToken)
        val adLoader = AdLoader()
        val adListener = UnityAdListener(resolvedPlacementId, requestToken)
        adLoaders[requestToken] = adLoader
        adListeners[requestToken] = adListener

        val adRequest = AdRequest.Builder(AdFormat.INTERSTITIAL)
            .setContext(context)
            .setPlacement(resolvedPlacementId)
            .setTestParams(
                mapOf(
                    "test_ad" to true,
                    "ad_network" to resolvedAdNetwork,
                )
            )
            .build()

        adLoader.loadAd(resolvedPlacementId, adListener, adRequest)
    }

    @JvmStatic
    fun loadAd(placementId: String, requestToken: String) {
        // Backward-compatible overload for older Unity C# layer.
        loadAd(placementId, requestToken, null)
    }

    @JvmStatic
    fun getAd(placementId: String, requestToken: String): Boolean {
        readyAds[requestToken]?.let { return it is InterstitialAd }
        val adLoader = adLoaders[requestToken] ?: return false
        val ad = adLoader.getAd(placementId) ?: return false
        if (ad !is InterstitialAd) {
            return false
        }
        readyAds[requestToken] = ad
        return true
    }

    @JvmStatic
    fun showAd(placementId: String, requestToken: String) {
        val activity = UnityPlayer.currentActivity
        if (activity == null) {
            sendShowError(placementId, requestToken, "Unity activity is null.")
            return
        }
        val ad = readyAds[requestToken]
        if (ad !is InterstitialAd) {
            Log.e(TAG, "showAd failed. placementId=$placementId requestToken=$requestToken cachedAd=${ad?.javaClass?.simpleName}")
            sendShowError(placementId, requestToken, "Interstitial ad is not available.")
            return
        }
        activity.runOnUiThread {
            Log.i(TAG, "showAd placementId=$placementId requestToken=$requestToken")
            ad.show(activity)
        }
    }

    private fun sendShowError(placementId: String, requestToken: String, error: String) {
        val payload = JSONObject()
            .put("placementId", placementId)
            .put("requestToken", requestToken)
            .put("error", error)
            .toString()
        UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_ERROR, payload)
    }

    private fun currentContext(): Context? = UnityPlayer.currentActivity?.applicationContext

    private class UnityAdListener(
        private val placementId: String,
        private val requestToken: String,
    ) : AdListener {
        override fun onAdLoaded(placementId: String, loadInfo: Map<String, Any>) {
            Log.i(TAG, "onAdLoaded placementId=$placementId loadInfo=$loadInfo")
            val adLoader = adLoaders[requestToken]
            val ad = adLoader?.getAd(placementId)
            if (ad != null) {
                readyAds[requestToken] = ad
                Log.i(TAG, "onAdLoaded cached ad type=${ad.javaClass.simpleName}")
            } else {
                Log.e(TAG, "onAdLoaded failed to cache ad for requestToken=$requestToken")
            }
            val payload = JSONObject()
                .put("placementId", placementId)
                .put("requestToken", requestToken)
                .put("loadInfo", JSONObject(loadInfo))
                .toString()
            UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_LOAD, payload)
        }

        override fun onError(msg: String, loadInfo: Map<String, Any>) {
            Log.e(TAG, "onError placementId=$placementId msg=$msg loadInfo=$loadInfo")
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
