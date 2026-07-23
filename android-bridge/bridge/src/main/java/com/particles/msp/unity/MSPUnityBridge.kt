package com.particles.msp.unity

import android.util.Log
import com.particles.msp.util.Logger
import com.particles.prebidadapter.MSP

object MSPUnityBridge {
    private const val TAG = "MSPUnityBridge"
    private val initializer = MSPUnityInitializer()
    private val adBridge = MSPUnityAdBridge()

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
        initializer.initialize(
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
        initializer.initializeJson(initializationJson)
    }

    @JvmStatic
    fun createAdLoader(): String = adBridge.createAdLoader()

    @JvmStatic
    fun destroyAdLoader(loaderId: String) {
        adBridge.destroyAdLoader(loaderId)
    }

    @JvmStatic
    fun loadAd(
        loaderId: String,
        placementId: String,
        customParamsJson: String?,
        testParamsJson: String?,
    ) {
        adBridge.loadAd(loaderId, placementId, customParamsJson, testParamsJson)
    }

    @JvmStatic
    fun getAd(loaderId: String, placementId: String): Boolean = adBridge.getAd(loaderId, placementId)

    @JvmStatic
    fun showAd(loaderId: String) {
        adBridge.showAd(loaderId)
    }
}
