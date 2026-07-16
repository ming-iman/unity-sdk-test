package com.particles.msp.unity

import android.content.Context
import android.util.Log
import com.particles.msp.api.AdFormat
import com.particles.msp.api.AdLoader
import com.particles.msp.api.AdRequest
import com.particles.msp.api.AdListener
import com.particles.msp.api.InterstitialAd
import com.particles.msp.api.MSPAd
import com.particles.msp.api.MSPInitListener
import com.particles.msp.api.MSPInitStatus
import com.particles.msp.api.MSPInitializationParameters
import com.particles.msp.util.HostConfig
import com.particles.msp.util.Logger
import com.particles.prebidadapter.MSP
import com.unity3d.player.UnityPlayer
import org.json.JSONObject
import org.prebid.mobile.Host
import org.prebid.mobile.PrebidMobile
import org.prebid.mobile.rendering.utils.helpers.AppInfoManager
import java.util.concurrent.ConcurrentHashMap

object MSPUnityBridge {
    private const val TAG = "MSPUnityBridge"
    private const val UNITY_GAME_OBJECT = "MSPUnityListener"
    private const val ON_INIT = "OnNativeInit"
    private const val ON_LOAD = "OnNativeLoad"
    private const val ON_EVENT = "OnNativeEvent"
    private const val ON_ERROR = "OnNativeError"
    private const val DEMO_INTERSTITIAL_PLACEMENT = "demo-android-interstitial"

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
        initialize(
            UnityInitializationConfig(
                prebidApiKey = prebidApiKey,
                orgId = orgId.toLong(),
                appId = appId.toLong(),
                isInTestMode = isInTestMode,
            )
        )
    }

    @JvmStatic
    fun initializeJson(initializationJson: String) {
        try {
            initialize(parseInitializationConfig(initializationJson))
        } catch (t: Throwable) {
            Log.e(TAG, "initializeJson failed: ${t.message}", t)
            sendInitResult("FAILURE", t.message ?: "Invalid MSP initialization JSON")
        }
    }

    private fun initialize(config: UnityInitializationConfig) {
        val activity = UnityPlayer.currentActivity
        if (activity == null) {
            Log.e(TAG, "initialize aborted: Unity activity is null")
            sendInitResult("FAILURE", "Unity activity is null")
            return
        }
        val context = activity.applicationContext
        val orgId = config.orgId.toMspIntId("orgId")
        val appId = config.appId.toMspIntId("appId")
        Log.i(TAG, "initialize called. orgId=${config.orgId} appId=${config.appId} testMode=${config.isInTestMode}")
        if (config.isInTestMode) {
            enableMesDebugLogging()
        }
        val mesInitUrl = HostConfig.getMesHostUrl(orgId).trimEnd('/') + "/v1/event/sdk_init"
        Log.i(TAG, "MES sdk_init endpoint: $mesInitUrl")

        // 4.5.0: consent APIs removed from AdapterParameters (resolved via IAB TCF stack).
        val initParams = object : MSPInitializationParameters {
            override fun getPrebidAPIKey(): String = config.prebidApiKey
            @Suppress("DEPRECATION")
            override fun getPrebidHostUrl(): String = config.resolvedPrebidHost()
            override fun getOrgId(): Int = orgId
            override fun getAppId(): Int = appId
            override fun getParameters(): Map<String, Any> = config.parameters
            override fun isAgeRestrictedUser(): Boolean = config.isAgeRestrictedUser
            override fun isInTestMode(): Boolean = config.isInTestMode
        }
        activity.runOnUiThread {
            // Prebid's initializer reloads identity while app name is still null.
            // Prime it first so the explicit profile overrides survive MSP.init().
            AppInfoManager.init(context)
            configureAndroidAppInfo(config)
            applyPrebidHost(config.prebidHost)
            MSP.init(context, initParams, object : MSPInitListener {
                override fun onComplete(status: MSPInitStatus, message: String) {
                    Log.i(TAG, "initialize complete. status=$status message=$message")
                    sendInitResult(status.name, message)
                }
            })
            // MSP Android 4.5.0's Prebid adapter still derives its host from orgId.
            // Restore the explicit app-profile host before any ad request is made.
            applyPrebidHost(config.prebidHost)
        }
    }

    private data class UnityInitializationConfig(
        val prebidApiKey: String,
        val sourceApp: String = "",
        val orgId: Long,
        val appId: Long,
        val prebidHost: String = "",
        val hasUserConsent: Boolean = true,
        val isAgeRestrictedUser: Boolean = false,
        val isDoNotSell: Boolean = false,
        val isInTestMode: Boolean = false,
        val consentString: String = "",
        val parameters: Map<String, Any> = emptyMap(),
        val appPackageName: String = "",
        val appVersionName: String = "",
    ) {
        fun resolvedPrebidHost(): String {
            if (prebidHost.isBlank()) {
                return HostConfig.getMspHostUrl(orgId.toMspIntId("orgId"))
            }
            val normalized = prebidHost.trimEnd('/')
            return if (normalized.endsWith("/openrtb2/auction")) {
                normalized
            } else {
                "$normalized/openrtb2/auction"
            }
        }
    }

    private fun parseInitializationConfig(json: String): UnityInitializationConfig {
        val obj = JSONObject(json)
        val parameters = obj.optJSONObject("parameters")?.let(::jsonObjectToMap) ?: emptyMap()
        return UnityInitializationConfig(
            prebidApiKey = obj.optString("prebidApiKey"),
            sourceApp = obj.optString("sourceApp"),
            orgId = obj.optLong("orgId"),
            appId = obj.optLong("appId"),
            prebidHost = obj.optString("prebidHost"),
            hasUserConsent = obj.optBoolean("hasUserConsent", true),
            isAgeRestrictedUser = obj.optBoolean("isAgeRestrictedUser"),
            isDoNotSell = obj.optBoolean("isDoNotSell"),
            isInTestMode = obj.optBoolean("isInTestMode"),
            consentString = obj.optString("consentString"),
            parameters = parameters,
            appPackageName = obj.optString("appPackageName"),
            appVersionName = obj.optString("appVersionName"),
        )
    }

    private fun Long.toMspIntId(name: String): Int {
        require(this in Int.MIN_VALUE.toLong()..Int.MAX_VALUE.toLong()) {
            "$name=$this exceeds the Int range supported by MSP Android 4.5.0"
        }
        return toInt()
    }

    private fun configureAndroidAppInfo(config: UnityInitializationConfig) {
        if (config.appPackageName.isNotBlank()) {
            AppInfoManager.setPackageName(config.appPackageName)
        }
        if (config.appVersionName.isNotBlank()) {
            try {
                val versionField = AppInfoManager::class.java.getDeclaredField("sAppVersion")
                versionField.isAccessible = true
                versionField.set(null, config.appVersionName)
            } catch (t: Throwable) {
                Log.w(TAG, "Unable to override Android app version: ${t.message}")
            }
        }
        if (config.sourceApp.isNotBlank()) {
            Log.d(TAG, "sourceApp received; MSP Android 4.5.0 has no source-app initialization API")
        }
        if (!config.hasUserConsent || config.isDoNotSell || config.consentString.isNotBlank()) {
            Log.d(TAG, "Consent fields received; MSP Android 4.5.0 resolves them from the IAB TCF/CMP state")
        }
    }

    private fun applyPrebidHost(prebidHost: String) {
        if (prebidHost.isBlank()) {
            return
        }
        Host.CUSTOM.hostUrl = prebidHost.trimEnd('/').let {
            if (it.endsWith("/openrtb2/auction")) it else "$it/openrtb2/auction"
        }
        PrebidMobile.setPrebidServerHost(Host.CUSTOM)
    }

    private fun sendInitResult(status: String, message: String) {
        val payload = JSONObject()
            .put("status", status)
            .put("message", message)
            .toString()
        UnityPlayer.UnitySendMessage(UNITY_GAME_OBJECT, ON_INIT, payload)
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
    fun loadAd(
        placementId: String,
        requestToken: String,
        customParamsJson: String?,
        testParamsJson: String?,
    ) {
        val context = currentContext() ?: return
        val resolvedPlacementId = if (placementId.isBlank()) DEMO_INTERSTITIAL_PLACEMENT else placementId
        val customParams = jsonToMap(customParamsJson)
        val testParams = jsonToMap(testParamsJson)
        Log.i(
            TAG,
            "loadAd called. placementId=$resolvedPlacementId requestToken=$requestToken " +
                "customParams=$customParams testParams=$testParams"
        )
        readyAds.remove(requestToken)
        val adLoader = AdLoader()
        val adListener = UnityAdListener(resolvedPlacementId, requestToken)
        adLoaders[requestToken] = adLoader
        adListeners[requestToken] = adListener

        val adRequest = AdRequest.Builder(AdFormat.INTERSTITIAL)
            .setContext(context)
            .setPlacement(resolvedPlacementId)
            .setCustomParams(customParams)
            .setTestParams(testParams)
            .build()

        adLoader.loadAd(resolvedPlacementId, adListener, adRequest)
    }

    @JvmStatic
    fun loadAd(placementId: String, requestToken: String) {
        // Backward-compatible overload for older Unity C# layer.
        loadAd(placementId, requestToken, null, null)
    }

    private fun jsonToMap(json: String?): Map<String, Any> {
        if (json.isNullOrBlank()) {
            return emptyMap()
        }
        return try {
            jsonObjectToMap(JSONObject(json))
        } catch (t: Throwable) {
            Log.w(TAG, "Failed to parse params JSON: ${t.message}")
            emptyMap()
        }
    }

    private fun jsonObjectToMap(obj: JSONObject): Map<String, Any> {
        val result = linkedMapOf<String, Any>()
        val keys = obj.keys()
        while (keys.hasNext()) {
            val key = keys.next()
            result[key] = jsonValueToAny(obj.get(key))
        }
        return result
    }

    private fun jsonValueToAny(value: Any?): Any {
        return when (value) {
            null, JSONObject.NULL -> JSONObject.NULL
            is JSONObject -> jsonObjectToMap(value)
            is org.json.JSONArray -> {
                val list = ArrayList<Any>(value.length())
                for (i in 0 until value.length()) {
                    list.add(jsonValueToAny(value.get(i)))
                }
                list
            }
            else -> value
        }
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
