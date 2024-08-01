using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements
{
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Applies the <paramref name="mask"/> to a copy of the <paramref name="texture"/> and returns the 
        /// resulting <see cref="Texture2D"/>.
        /// </summary>
        /// <remarks>
        /// Does not alter the given <paramref name="texture"/>.
        /// </remarks>
        /// <param name="texture"></param>
        /// <param name="mask"></param>
        /// <returns>A copy of the <paramref name="texture"/> with the <paramref name="mask"/> applied.</returns>
        /// <exception cref="InvalidOperationException">If the <paramref name="mask"/> does not have the same 
        /// <see cref="Texture2D.Bounds"/> as the <paramref name="texture"/>.</exception>
        public static Texture2D ApplyAlphaMask(this Texture2D texture, Texture2D mask)
        {
            Texture2D newTexture = texture.Duplicate();

            if (texture.Bounds != mask.Bounds)
            {
                throw new InvalidOperationException("Textures must have the same bounds to apply alpha mask.");
            }

            Color[] texturePixels = new Color[texture.Width * texture.Height];
            Color[] maskPixels = new Color[texture.Width * texture.Height];

            texture.GetData(texturePixels);
            mask.GetData(maskPixels);

            Color[] newPixels = new Color[texture.Width * texture.Height];
            for (int i = 0; i < newPixels.Length; i++)
            {
                Color texturePixel = texturePixels[i];
                Color maskPixel = maskPixels[i];

                byte alpha = Math.Min(GetValue(maskPixel), texturePixel.A);

                newPixels[i] = new Color(texturePixel.R, texturePixel.G, texturePixel.B, alpha);
            }

            newTexture.SetData(newPixels);

            return newTexture;
        }

        private static byte GetValue(Color color)
        {
            int sum = color.R + color.G + color.B;

            if (sum == 0)
            {
                return 0;
            }

            float average = (float)sum / 3.0f;

            return (byte)average;
        }
    }
}
