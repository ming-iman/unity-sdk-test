using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSP.Unity.Internal
{
    internal sealed class MSPUnityListener : MonoBehaviour
    {
        private static MSPUnityListener instance;
        private static Action<bool, string> pendingInitCallback;
        private static readonly Dictionary<string, MSPAdListener> listenersByLoaderId =
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

        internal static void RegisterLoadListener(string loaderId, MSPAdListener listener)
        {
            EnsureInstance();
            if (string.IsNullOrEmpty(loaderId) || listener == null)
            {
                return;
            }

            listenersByLoaderId[loaderId] = listener;
        }

        internal static void UnregisterLoadListener(string loaderId)
        {
            if (!string.IsNullOrEmpty(loaderId))
            {
                listenersByLoaderId.Remove(loaderId);
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
            if (message == null || string.IsNullOrEmpty(message.loaderId))
            {
                Debug.LogError("[MSPUnityListener] OnNativeLoad: invalid payload");
                return;
            }

            var listener = ResolveListener(message.loaderId);
            if (listener?.OnAdLoaded == null)
            {
                Debug.LogWarning(
                    $"[MSPUnityListener] OnNativeLoad: no listener for loaderId={message.loaderId}");
                return;
            }

            listener.OnAdLoaded(
                message.placementId,
                NativeBridgeMessages.ToLoadInfoDictionary(message.loadInfo));
        }

        public void OnNativeEvent(string payload)
        {
            Debug.Log($"[MSPUnityListener] OnNativeEvent: {payload}");
            var message = JsonUtility.FromJson<NativeEventMessage>(payload);
            if (message == null || string.IsNullOrEmpty(message.loaderId))
            {
                return;
            }

            var listener = ResolveListener(message.loaderId);
            if (listener == null)
            {
                return;
            }

            switch (message.@event)
            {
                case "clicked":
                case "click":
                    listener.OnAdClick?.Invoke(null);
                    break;
                case "impression":
                case "display":
                    listener.OnAdImpression?.Invoke(null);
                    break;
                case "dismissed":
                case "hide":
                    listener.OnAdDismissed?.Invoke(null);
                    break;
            }
        }

        public void OnNativeError(string payload)
        {
            Debug.LogError($"[MSPUnityListener] OnNativeError: {payload}");
            var message = JsonUtility.FromJson<NativeErrorMessage>(payload);
            if (message == null || string.IsNullOrEmpty(message.loaderId))
            {
                return;
            }

            var listener = ResolveListener(message.loaderId);
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
        }

        private static MSPAdListener ResolveListener(string loaderId)
        {
            if (!string.IsNullOrEmpty(loaderId) &&
                listenersByLoaderId.TryGetValue(loaderId, out var listener))
            {
                return listener;
            }

            return null;
        }
    }
}
