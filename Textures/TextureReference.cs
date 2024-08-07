using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Textures
{
    public class TextureReference : IDisposable
    {
        private static Logger Logger = Logger.GetLogger<TextureReference>();

        private bool _disposed;

        private int? _assetId;
        private bool _applySelfMask = false;

        private Texture2D _disposableTexture;
        private AsyncTexture2D _assetCacheReference;

        /// <remarks>
        /// Will never be <see langword="null"/>, even after it was disposed.
        /// </remarks>
        public Texture2D Texture
        {
            get
            {
                if (_disposed)
                {
                    return ContentService.Textures.Error;
                }
                
                if (_disposableTexture != null)
                {
                    return _disposableTexture;
                }

                if (!_assetId.HasValue)
                {
                    Logger.Warn($"TextureReference is not disposed, but {nameof(_disposableTexture)} and {nameof(_assetId)} are null.");
                    return ContentService.Textures.Error;
                }

                if (_assetCacheReference?.IsDisposed == false)
                {
                    return _assetCacheReference.Texture;
                }

                if (_assetCacheReference == null)
                {
                    Logger.Warn($"TextureReference is not disposed, but {nameof(_disposableTexture)} and {nameof(_assetCacheReference)} are null.");
                }

                if (_assetCacheReference?.IsDisposed == true)
                {
                    Logger.Warn($"AssetCache reference has been disposed (possibly by another module). Retrieving new reference.");
                }

                RetrieveAssetCacheReference(_assetId.Value, _applySelfMask);

                return _disposableTexture ?? _assetCacheReference.Texture ?? ContentService.Textures.Error;
            }
        }

        /// <summary>
        /// Determines whether the <see cref="TextureReference"/> has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Determines whether the <see cref="TextureReference"/> is resolved. Will always be <see langword="true"/> for local textures.
        /// </summary>
        /// <remarks>
        /// Will return <see langword="false"/>, if the <see cref="TextureReference"/> has been disposed.
        /// </remarks>
        public bool IsResolved
        {
            get
            {
                if (_disposed)
                {
                    return false; // No need to throw here.
                    // TODO: document this behaviour!
                }

                if (_disposableTexture != null)
                {
                    return true;
                }

                if (_assetCacheReference == null)
                {
                    return false; // TODO: maybe throw here. This should not happen.
                }

                return _assetCacheReference.HasSwapped;
            }
        }

        /// <summary>
        /// Fires, when the <see cref="TextureReference"/> has been resolved. Will only fire for <see cref="TextureReference"/>s that are 
        /// instantiated with an assetId, since local textures will be resolved synchronously.
        /// </summary>
        public event EventHandler Resolved;

        private void OnResolved()
        {
            Resolved?.Invoke(this, EventArgs.Empty);
        }

        private void RetrieveAssetCacheReference(int assetId, bool applySelfMask)
        {
            if (_assetCacheReference != null && !_assetCacheReference.IsDisposed)
            {
                return;
            }
            CleanUpAssetCacheReference();

            _assetCacheReference = GameService.Content.DatAssetCache.GetTextureFromAssetId(assetId);
            _assetId = assetId;
            _applySelfMask = applySelfMask;

            if (applySelfMask)
            {
                if (_assetCacheReference.HasSwapped)
                {
                    OnTextureSwappedApplyMask(null, null);
                }
                else
                {
                    _assetCacheReference.TextureSwapped += OnTextureSwappedApplyMask;
                }
            }
            else
            {
                if (_assetCacheReference.HasSwapped)
                {
                    OnTextureSwapped(null, null);
                }
                else
                {
                    _assetCacheReference.TextureSwapped += OnTextureSwapped;
                }
            }
        }

        /// <summary>
        /// Returns an awaitable <see cref="Task"/> that contains the <see cref="Texture"/>, after 
        /// it was resolved.
        /// </summary>
        /// <remarks>
        /// Will syncronously return, if the <see cref="Texture"/> is local.
        /// </remarks>
        /// <returns>A <see cref="Task"/> with the resolved <see cref="Texture2D"/>.</returns>
        public async Task<Texture2D> WaitUntilResolved()
        {
            if (IsResolved)
            {
                return Texture;
            }

            TaskCompletionSource<Texture2D> completionSource = new TaskCompletionSource<Texture2D>();

            EventHandler resolvedEventHandler = new EventHandler((o, s) => completionSource.SetResult(Texture));

            Resolved += resolvedEventHandler;

            await completionSource.Task;

            Resolved -= resolvedEventHandler;

            return Texture;
        }

        public TextureReference(int assetId, bool applySelfMask)
        {
            RetrieveAssetCacheReference(assetId, applySelfMask);
        }

        public TextureReference(int assetId) : this (assetId, false) { /** NOOP **/ }

        public TextureReference(string localTexture)
        {
            _disposableTexture = AdditionalAchievementsModule.Instance.ContentsManager.GetTexture(localTexture);
        }

        public void ApplyAlphaMask(TextureReference alphaMask, bool disposeMask)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            
            Texture2D currentTexture = _disposableTexture;

            _disposableTexture = Texture.ApplyAlphaMask(alphaMask.Texture);
            
            currentTexture?.Dispose();

            if (disposeMask)
            {
                alphaMask?.Dispose();
            }
        }

        private void OnTextureSwapped(object _, ValueChangedEventArgs<Texture2D> _1)
        {
            OnResolved();
        }

        private void OnTextureSwappedApplyMask(object _, ValueChangedEventArgs<Texture2D> _1)
        {
            _disposableTexture = _assetCacheReference.Texture.ApplyAlphaMask(_assetCacheReference);
            OnTextureSwapped(null, null);
        }

        private void CleanUpAssetCacheReference()
        {
            if (_assetCacheReference != null)
            {
                _assetCacheReference.TextureSwapped -= OnTextureSwappedApplyMask;
                _assetCacheReference.TextureSwapped -= OnTextureSwapped;
                _assetCacheReference = null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            
            CleanUpAssetCacheReference();

            _disposableTexture?.Dispose();
            _disposableTexture = null;

            Resolved = null;
        }

        public static implicit operator Texture2D(TextureReference textureReference)
        {
            return textureReference.Texture;
        }
    }
}
