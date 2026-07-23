package com.particles.msp.unity

import com.unity3d.player.UnityPlayer
import org.json.JSONObject

internal object MSPUnityMessages {
    fun sendInitResult(status: String, message: String) {
        val payload = JSONObject()
            .put("status", status)
            .put("message", message)
            .toString()
        UnityPlayer.UnitySendMessage(
            MSPUnityBridgeConstants.UNITY_GAME_OBJECT,
            MSPUnityBridgeConstants.ON_INIT,
            payload
        )
    }

    fun sendLoad(placementId: String, loaderId: String, loadInfo: Map<String, Any>) {
        val payload = JSONObject()
            .put("placementId", placementId)
            .put("loaderId", loaderId)
            .put("loadInfo", JSONObject(loadInfo))
            .toString()
        UnityPlayer.UnitySendMessage(
            MSPUnityBridgeConstants.UNITY_GAME_OBJECT,
            MSPUnityBridgeConstants.ON_LOAD,
            payload
        )
    }

    fun sendError(placementId: String, loaderId: String, error: String, loadInfo: Map<String, Any>? = null) {
        val payload = JSONObject()
            .put("placementId", placementId)
            .put("loaderId", loaderId)
            .put("error", error)
            .apply {
                if (loadInfo != null) {
                    put("loadInfo", JSONObject(loadInfo))
                }
            }
            .toString()
        UnityPlayer.UnitySendMessage(
            MSPUnityBridgeConstants.UNITY_GAME_OBJECT,
            MSPUnityBridgeConstants.ON_ERROR,
            payload
        )
    }

    fun sendLifecycle(placementId: String, loaderId: String, eventName: String) {
        val payload = JSONObject()
            .put("placementId", placementId)
            .put("loaderId", loaderId)
            .put("event", eventName)
            .toString()
        UnityPlayer.UnitySendMessage(
            MSPUnityBridgeConstants.UNITY_GAME_OBJECT,
            MSPUnityBridgeConstants.ON_EVENT,
            payload
        )
    }
}
