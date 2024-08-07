using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Notification : IDisposable
    {
        private readonly TextureReference _shine;
        private readonly TextureReference _shine2;
        private readonly TextureReference _background;
        private readonly TextureReference _defaultAchievement;

        public Texture2D Shine => _shine;
        public Texture2D Shine2 => _shine2;
        public Texture2D Background => _background;
        public Texture2D DefaultAchievement => _defaultAchievement;

        public Notification()
        {
            _shine = new TextureReference(255225, true);
            _shine2 = new TextureReference(255227, true);

            _background = new TextureReference("png/achievementNotifBackground214x70.png");

            _defaultAchievement = new TextureReference(42684);
        }

        public async Task WaitUntilResolved()
        {
            await _shine.WaitUntilResolved();
            await _shine2.WaitUntilResolved();

            await _background.WaitUntilResolved();

            await _defaultAchievement.WaitUntilResolved();
        }

        public void Dispose()
        {
            _shine?.Dispose();

            _shine2?.Dispose();

            _background?.Dispose();

            _defaultAchievement?.Dispose();
        }
    }
}
