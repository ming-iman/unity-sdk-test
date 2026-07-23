package com.particles.msp.unity

import android.util.Log
import com.particles.msp.api.MSPInitListener
import com.particles.msp.api.MSPInitStatus
import com.particles.msp.api.MSPInitializationParameters
import com.particles.msp.util.HostConfig
import com.particles.prebidadapter.MSP
import com.unity3d.player.UnityPlayer
import org.json.JSONObject
import org.prebid.mobile.Host
import org.prebid.mobile.PrebidMobile
import org.prebid.mobile.rendering.utils.helpers.AppInfoManager

internal class MSPUnityInitializer {
    fun initializeJson(initializationJson: String) {
        try {
            initialize(parseInitializationConfig(initializationJson))
        } catch (t: Throwable) {
            Log.e(MSPUnityBridgeConstants.TAG, "initializeJson failed: ${t.message}", t)
            MSPUnityMessages.sendInitResult("FAILURE", t.message ?: "Invalid MSP initialization JSON")
        }
    }

    fun initialize(config: UnityInitializationConfig) {
        val activity = UnityPlayer.currentActivity
        if (activity == null) {
            Log.e(MSPUnityBridgeConstants.TAG, "initialize aborted: Unity activity is null")
            MSPUnityMessages.sendInitResult("FAILURE", "Unity activity is null")
            return
        }
        val context = activity.applicationContext
        val orgId = config.orgId.toMspIntId("orgId")
        val appId = config.appId.toMspIntId("appId")
        Log.i(
            MSPUnityBridgeConstants.TAG,
            "initialize called. orgId=${config.orgId} appId=${config.appId} testMode=${config.isInTestMode}"
        )
        if (config.isInTestMode) {
            enableMesDebugLogging()
        }
        val mesInitUrl = HostConfig.getMesHostUrl(orgId).trimEnd('/') + "/v1/event/sdk_init"
        Log.i(MSPUnityBridgeConstants.TAG, "MES sdk_init endpoint: $mesInitUrl")

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
            AppInfoManager.init(context)
            configureAndroidAppInfo(config)
            applyPrebidHost(config.prebidHost)
            MSP.init(context, initParams, object : MSPInitListener {
                override fun onComplete(status: MSPInitStatus, message: String) {
                    Log.i(MSPUnityBridgeConstants.TAG, "initialize complete. status=$status message=$message")
                    MSPUnityMessages.sendInitResult(status.name, message)
                }
            })
            applyPrebidHost(config.prebidHost)
        }
    }

    private fun parseInitializationConfig(json: String): UnityInitializationConfig {
        val obj = JSONObject(json)
        val parameters = obj.optJSONObject("parameters")?.let(MSPUnityJson::jsonObjectToMap) ?: emptyMap()
        return UnityInitializationConfig(
            prebidApiKey = obj.optString("prebidApiKey"),
            sourceApp = obj.optString("sourceApp"),
            orgId = obj.optLong("orgId"),
            appId = obj.optLong("appId"),
            prebidHost = obj.optString("prebidHost"),
            isAgeRestrictedUser = obj.optBoolean("isAgeRestrictedUser"),
            isInTestMode = obj.optBoolean("isInTestMode"),
            parameters = parameters,
            appPackageName = obj.optString("appPackageName"),
            appVersionName = obj.optString("appVersionName"),
        )
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
                Log.w(MSPUnityBridgeConstants.TAG, "Unable to override Android app version: ${t.message}")
            }
        }
        if (config.sourceApp.isNotBlank()) {
            Log.d(
                MSPUnityBridgeConstants.TAG,
                "sourceApp received; MSP Android 4.5.0 has no source-app initialization API"
            )
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

    private fun enableMesDebugLogging() {
        try {
            val clazz = Class.forName("com.particles.mes.android.MesConfig")
            val field = clazz.getDeclaredField("debug")
            field.isAccessible = true
            field.setBoolean(null, true)
            Log.i(MSPUnityBridgeConstants.TAG, "MesConfig.debug enabled for test mode")
        } catch (t: Throwable) {
            Log.w(MSPUnityBridgeConstants.TAG, "Unable to enable MesConfig.debug: ${t.message}")
        }
    }
}

internal data class UnityInitializationConfig(
    val prebidApiKey: String,
    val sourceApp: String = "",
    val orgId: Long,
    val appId: Long,
    val prebidHost: String = "",
    val isAgeRestrictedUser: Boolean = false,
    val isInTestMode: Boolean = false,
    val parameters: Map<String, Any> = emptyMap(),
    val appPackageName: String = "",
    val appVersionName: String = "",
) {
    fun resolvedPrebidHost(): String {
        if (prebidHost.isBlank()) {
            return HostConfig.getMspHostUrl(orgId.toMspIntId("orgId"))
        }
        val normalized = prebidHost.trimEnd('/')
        return if (normalized.endsWith("/openrtb2/auction")) normalized else "$normalized/openrtb2/auction"
    }
}

internal fun Long.toMspIntId(name: String): Int {
    require(this in Int.MIN_VALUE.toLong()..Int.MAX_VALUE.toLong()) {
        "$name=$this exceeds the Int range supported by MSP Android 4.5.0"
    }
    return toInt()
}
