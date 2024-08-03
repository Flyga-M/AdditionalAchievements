using System;
using Microsoft.Xna.Framework.Graphics;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Display : IDisposable
    {
        private readonly Progress _progress;
        private readonly Back _back;
        private readonly Selection _selection;
        private readonly Description _description;
        private readonly Repo _repo;

        private readonly TextureReference _moduleIconCorner;
        private readonly TextureReference _moduleIconCornerHover;
        private readonly TextureReference _moduleEmblem;

        private readonly TextureReference _achievementWindowBackground;

        private readonly TextureReference _tabAchievementsIcon;
        private readonly TextureReference _tabRepositoryIcon;
        private readonly TextureReference _tabStatusIcon;

        private readonly TextureReference _watchIcon;
        private readonly TextureReference _watchIconSelected;
        private readonly TextureReference _watchBackgroundHighlight;

        public Progress Progress => _progress;

        public Back Back => _back;

        public Selection Selection => _selection;

        public Description Description => _description;

        public Repo Repo => _repo;

        /// <summary>
        /// The corner icon for the module.
        /// </summary>
        public Texture2D IconCorner => _moduleIconCorner;

        /// <summary>
        /// The corner icon for the module when it's hovered.
        /// </summary>
        public Texture2D IconCornerHover => _moduleIconCornerHover;

        /// <summary>
        /// The module emblem, that is used for windows.
        /// </summary>
        public Texture2D Emblem => _moduleEmblem;

        /// <summary>
        /// The background image for the main window.
        /// </summary>
        public Texture2D AchievementWindowBackground => _achievementWindowBackground;

        /// <summary>
        /// The tab icon for the status tab.
        /// </summary>
        public Texture2D TabStatusIcon => _tabStatusIcon;

        /// <summary>
        /// The tab icon for the repository tab.
        /// </summary>
        public Texture2D TabRepositoryIcon => _tabRepositoryIcon;

        /// <summary>
        /// The tab icon for the achievements tab.
        /// </summary>
        public Texture2D TabAchievementsIcon => _tabAchievementsIcon;

        public Texture2D WatchIcon => _watchIcon;
        public Texture2D WatchIconSelected => _watchIconSelected;
        public Texture2D WatchBackgroundHighlight => _watchBackgroundHighlight;

        public Display()
        {
            _progress = new Progress();
            _back = new Back();
            _selection = new Selection();
            _description = new Description();
            _repo = new Repo();

            _moduleIconCorner = new TextureReference("png/moduleIcon64.png");
            _moduleIconCornerHover = new TextureReference("png/moduleIcon_grown64.png");

            _moduleEmblem = new TextureReference("png/moduleEmblem.png");

            _achievementWindowBackground = new TextureReference(155985);

            _tabStatusIcon = new TextureReference(156737);
            _tabRepositoryIcon = new TextureReference(156702);
            _tabAchievementsIcon = new TextureReference(156709);

            _watchIcon = new TextureReference(605021);
            _watchIconSelected = new TextureReference(605019);
            _watchBackgroundHighlight = new TextureReference(605000);
        }

        public void Dispose()
        {
            _progress?.Dispose();
            _back?.Dispose();
            _selection?.Dispose();
            _description?.Dispose();
            _repo?.Dispose();

            _moduleIconCorner?.Dispose();

            _moduleIconCornerHover?.Dispose();

            _moduleEmblem?.Dispose();

            _achievementWindowBackground?.Dispose();

            _tabStatusIcon?.Dispose();
            _tabRepositoryIcon?.Dispose();
            _tabAchievementsIcon?.Dispose();

            _watchIcon?.Dispose();
            _watchIconSelected?.Dispose();
            _watchBackgroundHighlight?.Dispose();
        }
    }
}
