using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Back : IDisposable
    {
        private  readonly TextureReference _background;
        private readonly TextureReference _backArrow;

        public Texture2D Background => _background;

        public Texture2D Arrow => _backArrow;

        public Back()
        {
            // new: 1032327
            // old: 784279
            _background = new TextureReference(1032327);
            _backArrow = new TextureReference(784268);
        }

        public void Dispose()
        {
            _background?.Dispose();

            _backArrow?.Dispose();
        }
    }
}
