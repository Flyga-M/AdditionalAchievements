using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Integration
{
    public class AssetResolveContext : ITextureResolveContext
    {
        public bool CanResolve(object resolvable)
        {
            if (resolvable == null)
            {
                return false;
            }

            return typeof(IResolvableTexture).IsAssignableFrom(resolvable.GetType());
        }

        public Texture2D Resolve(IResolvableTexture resolvable)
        {
            if (resolvable == null)
            {
                throw new ArgumentNullException(nameof(resolvable));
            }

            AsyncTexture2D texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(resolvable.AssetId);

            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            if (texture.HasSwapped)
            {
                return texture;
            }
            else
            {
                texture.TextureSwapped += (s, e) => completionSource.SetResult(e.NewValue != null);
            }

            // TODO: give user feedback if this time is exceeded
            completionSource.Task.Wait(2_000);

            return texture;
        }

        public object Resolve(object resolvable)
        {
            if (resolvable == null)
            {
                throw new ArgumentNullException(nameof(resolvable));
            }

            if (!CanResolve(resolvable))
            {
                throw new ArgumentException($"Resolvable of type {resolvable.GetType()} "
                    + $"can't be resolved by the {nameof(AssetResolveContext)}. " +
                    $"Expected Type: {typeof(IResolvableTexture)}.", nameof(resolvable));
            }

            return Resolve(resolvable as IResolvableTexture);
        }

        public bool TryResolve(IResolvableTexture resolvable, out Texture2D resolved)
        {
            resolved = null;

            try
            {
                resolved = Resolve(resolvable);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool TryResolve(object resolvable, out object resolved)
        {
            bool eval = TryResolve(resolvable as IResolvableTexture, out Texture2D resolvedTexture);
            resolved = resolvedTexture;
            return eval;
        }
    }
}
