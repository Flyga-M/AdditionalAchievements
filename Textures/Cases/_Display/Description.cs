using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Description : IDisposable
    {
        private readonly TextureReference _verticalDivider;
        private readonly TextureReference _titleBackground;

        private readonly TextureReference _completedBackgroundHighlight;

        private readonly TextureReference _checkmarkGreen;
        private readonly TextureReference _dash;

        public Texture2D VerticalDivider => _verticalDivider;
        public Texture2D TitleBackground => _titleBackground;

        public Texture2D CompletedBackgroundHighlight => _completedBackgroundHighlight;

        public Texture2D CheckmarkGreen => _checkmarkGreen;
        public Texture2D Dash => _dash;

        public Description()
        {
            _verticalDivider = new TextureReference(870380);
            _titleBackground = new TextureReference(784279);

            _completedBackgroundHighlight = new TextureReference(605007);

            _checkmarkGreen = new TextureReference(154979);
            _dash = new TextureReference(255300);
        }

        public async Task WaitUntilResolved()
        {
            await _verticalDivider.WaitUntilResolved();
            await _titleBackground.WaitUntilResolved();
            await _completedBackgroundHighlight.WaitUntilResolved();
            await _checkmarkGreen.WaitUntilResolved();
            await _dash.WaitUntilResolved();
        }

        public void Dispose()
        {
            _verticalDivider?.Dispose();
            _titleBackground?.Dispose();

            _completedBackgroundHighlight?.Dispose();

            _checkmarkGreen?.Dispose();
            _dash?.Dispose();
        }
    }
}
