using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSP.Unity.Internal
{
    internal sealed class MSPUnityListener : MonoBehaviour
    {
        private static MSPUnityListener instance;
        private static Action<bool, string> pendingInitCallback;
        private static readonly Dictionary<string, MSPAdListener> listenersByToken =
            new Dictionary<string, MSPAdListener>();
        private static readonly Dictionary<string, MSPAdListener> listenersByPlacement =
            new Dictionary<string, MSPAdListener>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            var go = new GameObject("MSPUnityListener");
            instance = go.AddComponent<MSPUnityListener>();
            DontDestroyOnLoad(go);
        }

        internal static void SetPendingInitCallback(Action<bool, string> callback)
        {
            EnsureInstance();
            pendingInitCallback = callback;
        }

        internal static void RegisterLoadListener(
            string requestToken,
            string placementId,
            MSPAdListener listener)
        {
            EnsureInstance();
            if (string.IsNullOrEmpty(requestToken) || listener == null)
            {
                return;
            }

            listenersByToken[requestToken] = listener;
            listenersByPlacement[placementId] = listener;
        }

        internal static void UnregisterLoadListener(string requestToken, string placementId)
        {
            if (!string.IsNullOrEmpty(requestToken))
            {
                listenersByToken.Remove(requestToken);
            }

            if (!string.IsNullOrEmpty(placementId))
            {
                listenersByPlacement.Remove(placementId);
            }
        }

        public void OnNativeInit(string payload)
        {
            var success = payload != null && payload.Contains("\"status\":\"SUCCESS\"");
            pendingInitCallback?.Invoke(success, payload ?? string.Empty);
            pendingInitCallback = null;
        }

        public void OnNativeLoad(string payload)
        {
            Debug.Log($"[MSPUnityListener] OnNativeLoad: {payload}");
            var message = JsonUtility.FromJson<NativeLoadMessage>(payload);
            if (message == null || string.IsNullOrEmpty(message.placementId))
            {
                Debug.LogError("[MSPUnityListener] OnNativeLoad: invalid payload");
                return;
            }

            var listener = ResolveListener(message.requestToken, message.placementId);
            if (listener?.OnAdLoaded == null)
            {
                Debug.LogWarning(
                    $"[MSPUnityListener] OnNativeLoad: no listener for placement={message.placementId}");
                return;
            }

            listener.OnAdLoaded(
                message.placementId,
                NativeBridgeMessages.ToLoadInfoDictionary(message.loadInfo));
            UnregisterLoadListener(message.requestToken, message.placementId);
        }

        public void OnNativeEvent(string payload)
        {
            Debug.Log($"[MSPUnityListener] OnNativeEvent: {payload}");
        }

        public void OnNativeError(string payload)
        {
            Debug.LogError($"[MSPUnityListener] OnNativeError: {payload}");
            var message = JsonUtility.FromJson<NativeErrorMessage>(payload);
            if (message == null || string.IsNullOrEmpty(message.placementId))
            {
                return;
            }

            var listener = ResolveListener(message.requestToken, message.placementId);
            if (listener?.OnError == null)
            {
                return;
            }

            var loadInfo = NativeBridgeMessages.ToLoadInfoDictionary(message.loadInfo);
            if (!string.IsNullOrEmpty(message.error))
            {
                loadInfo["error"] = message.error;
            }

            listener.OnError(message.error ?? "Unknown error", loadInfo);
            UnregisterLoadListener(message.requestToken, message.placementId);
        }

        private static MSPAdListener ResolveListener(string requestToken, string placementId)
        {
            if (!string.IsNullOrEmpty(requestToken) &&
                listenersByToken.TryGetValue(requestToken, out var byToken))
            {
                return byToken;
            }

            if (!string.IsNullOrEmpty(placementId) &&
                listenersByPlacement.TryGetValue(placementId, out var byPlacement))
            {
                return byPlacement;
            }

            return null;
        }
    }
}
