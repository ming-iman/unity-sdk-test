using System;
using MSP.Unity.Internal;

namespace MSP.Unity
{
    /// <summary>
    /// Thin wrapper around the native AdLoader / MSPAdLoader.
    /// Prefer creating a new instance for each ad load.
    /// </summary>
    public sealed class MSPAdLoader : IDisposable
    {
        private readonly IMSPClient client;
        private readonly string loaderId;
        private MSPAdListener adListener;
        private bool disposed;

        public MSPAdLoader()
        {
            client = MSPClientFactory.Create();
            loaderId = client.CreateAdLoader();
        }

        public void LoadAd(string placementId, MSPAdListener adListener, MSPAdRequest adRequest)
        {
            ThrowIfDisposed();
            if (adRequest == null)
            {
                throw new ArgumentNullException(nameof(adRequest));
            }

            this.adListener = adListener;
            client.LoadAd(loaderId, placementId, adRequest, adListener);
        }

        public MSPAd GetAd(string placementId)
        {
            ThrowIfDisposed();
            var ad = client.GetAd(loaderId, placementId, adListener);
            if (ad != null)
            {
                MSPUnityListener.RegisterAd(loaderId, ad);
            }
            return ad;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            MSPUnityListener.UnregisterLoadListener(loaderId);
            client.DestroyAdLoader(loaderId);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(MSPAdLoader));
            }
        }
    }
}
