using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Description : IDisposable
    {
        private readonly TextureReference _verticalDivider;
        private readonly TextureReference _titleBackground;

        private readonly TextureReference _completedBackgroundHighlight;

        public Texture2D VerticalDivider => _verticalDivider;
        public Texture2D TitleBackground => _titleBackground;

        public Texture2D CompletedBackgroundHighlight => _completedBackgroundHighlight;

        public Description()
        {
            _verticalDivider = new TextureReference(870380);
            _titleBackground = new TextureReference(784279);

            _completedBackgroundHighlight = new TextureReference(605007);
        }

        public void Dispose()
        {
            _verticalDivider?.Dispose();

            _titleBackground?.Dispose();

            _completedBackgroundHighlight?.Dispose();
        }
    }
}
