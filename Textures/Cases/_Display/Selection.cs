using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Selection : IDisposable
    {
        private readonly TextureReference _bottomSeparator;

        private readonly TextureReference _completedShineHighlight;
        private readonly TextureReference _completedShine2Highlight;
        private readonly TextureReference _completedBackgroundHighlight;

        public Texture2D BottomSeparator => _bottomSeparator;

        public Texture2D CompletedShineHighlight => _completedShineHighlight;
        public Texture2D CompletedShine2Highlight => _completedShine2Highlight;
        public Texture2D CompletedBackgroundHighlight => _completedBackgroundHighlight;

        public Selection()
        {
            _bottomSeparator = new TextureReference(157218);

            _completedShineHighlight = new TextureReference(605008);
            _completedShine2Highlight = new TextureReference(605009);
            _completedBackgroundHighlight = new TextureReference(605007);
        }

        public void Dispose()
        {
            _bottomSeparator?.Dispose();

            _completedShineHighlight?.Dispose();
            _completedShine2Highlight?.Dispose();
            _completedBackgroundHighlight?.Dispose();
        }
    }
}
