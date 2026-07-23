package com.particles.msp.unity

import android.util.Log
import org.json.JSONArray
import org.json.JSONObject

internal object MSPUnityJson {
    fun jsonToMap(json: String?): Map<String, Any> {
        if (json.isNullOrBlank()) {
            return emptyMap()
        }
        return try {
            jsonObjectToMap(JSONObject(json))
        } catch (t: Throwable) {
            Log.w(MSPUnityBridgeConstants.TAG, "Failed to parse params JSON: ${t.message}")
            emptyMap()
        }
    }

    fun jsonObjectToMap(obj: JSONObject): Map<String, Any> {
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
            is JSONArray -> {
                val list = ArrayList<Any>(value.length())
                for (i in 0 until value.length()) {
                    list.add(jsonValueToAny(value.get(i)))
                }
                list
            }
            else -> value
        }
    }
}
