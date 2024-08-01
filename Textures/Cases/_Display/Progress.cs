using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Progress : IDisposable
    {
        private readonly TextureReference _fillCrest;
        private readonly TextureReference _vignette;
        private readonly TextureReference _tierBackground;

        public Texture2D FillCrest => _fillCrest;
        public Texture2D Vignette => _vignette;
        public Texture2D TierBackground => _tierBackground;

        public Progress()
        {
            _fillCrest = new TextureReference(605004, true);
            _vignette = new TextureReference(605003);
            _tierBackground = new TextureReference("png/achievementTier.png");
        }

        public void Dispose()
        {
            _fillCrest?.Dispose();

            _vignette?.Dispose();

            _tierBackground?.Dispose();
        }
    }
}
