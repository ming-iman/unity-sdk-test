package com.particles.msp.unity

/**
 * Android bridge skeleton for Unity plugin.
 * Final integration should call msp-android MSP + AdLoader APIs directly.
 */
object MSPUnityBridge {
    @JvmStatic
    fun getVersion(): String = "android-bridge-skeleton"

    @JvmStatic
    fun initialize(prebidApiKey: String, orgId: Int, appId: Int, isInTestMode: Boolean) {
        // TODO: wire to com.particles.prebidadapter.MSP.init(...)
    }

    @JvmStatic
    fun loadInterstitial(placementId: String, requestToken: String) {
        // TODO: wire to AdLoader.loadAd(...) and return callbacks to Unity.
    }

    @JvmStatic
    fun showInterstitial(placementId: String, requestToken: String) {
        // TODO: fetch ad via AdLoader.getAd(placementId) and show activity modal.
    }
}
