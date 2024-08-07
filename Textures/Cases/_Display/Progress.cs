using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Progress : IDisposable
    {
        private readonly TextureReference _fillCrest;
        private readonly TextureReference _vignette;
        private readonly TextureReference _tierBackground;
        private readonly TextureReference _lock;

        public Texture2D FillCrest => _fillCrest;
        public Texture2D Vignette => _vignette;
        public Texture2D TierBackground => _tierBackground;
        public Texture2D Lock => _lock;

        public Progress()
        {
            _fillCrest = new TextureReference(605004, true);
            _vignette = new TextureReference(605003);
            _tierBackground = new TextureReference("png/achievementTier.png");
            _lock = new TextureReference(240704);
        }

        public async Task WaitUntilResolved()
        {
            await _fillCrest.WaitUntilResolved();
            await _vignette.WaitUntilResolved();
            await _tierBackground.WaitUntilResolved();
            await _lock.WaitUntilResolved();
        }

        public void Dispose()
        {
            _fillCrest?.Dispose();

            _vignette?.Dispose();

            _tierBackground?.Dispose();

            _lock?.Dispose();
        }
    }
}
