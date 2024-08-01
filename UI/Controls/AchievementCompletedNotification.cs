using Blish_HUD;
using Blish_HUD.Controls;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Flyga.AdditionalAchievements.Textures;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Attempts to recreate the screen notification, that is shown when an achievement is completed.
    /// </summary>
    public class AchievementCompletedNotification : Control
    {
        private readonly Texture2D _achievementIcon;
        private readonly string _achievementName;

        private const float TOTAL_DURATION = 6.0f;

        private const float SHINE_ROTATION_TARGET = MathHelper.Pi * 2 * 5;
        private const float SHINE_ROTATION_DURATION = TOTAL_DURATION;

        private const float SHINE_OPACITY_TARGET = 1.0f;
        private const float SHINE_OPACITY_DURATION = 0.3f;

        private const float SHINE_SCALE_TARGET = 1.0f;
        private const float SHINE_SCALE_DURATION = 0.7f;

        private float _shineRotation = 0.0f;
        private float _shineOpacity = 0.0f;
        private float _shineScale = 3.0f;

        private const int ICON_OFFSET_LEFT = 16;

        private const float ICON_OPACITY_TARGET = 1.0f;
        private const float ICON_OPACITY_DURATION = 0.3f;
        private const float ICON_OPACITY_DELAY = 0.1f;

        private const float ICON_SCALE_TARGET = 1.0f;
        private const float ICON_SCALE_DURATION = SHINE_SCALE_DURATION + 0.5f;

        private float _iconOpacity = 0.0f;
        private float _iconScale = 3.0f;

        private const float FADEOUT_DURATION = 1.0f;
        private static float FADEOUT_DELAY => TOTAL_DURATION - FADEOUT_DURATION;

        private float BackgroundOpacity => _shineOpacity;

        private Vector2 IconCenter
        {
            get
            {
                float size = (float)Height;


                float x = size / 2.0f;
                float y = size / 2.0f;

                return new Vector2(x, y);
            }
        }

        private Rectangle IconBounds
        {
            get
            {
                return ShineBounds.ScaleBy(0.85f);
            }
        }

        private Rectangle ShineBounds
        {
            get
            {
                int size = Height;
                return new Rectangle(0, 0, size, size);
            }
        }

        private Rectangle TextBounds
        {
            get
            {
                Rectangle shineBounds = ShineBounds;


                int x = shineBounds.Right + 4;
                int y = shineBounds.Top + 4;

                int width = Width - shineBounds.Width - 4;
                int height = Height - 4;

                return new Rectangle(x, y, width, height);
            }
        }

        public event EventHandler LifetimeEnd;

        private void OnLifetimeEnd()
        {
            LifetimeEnd?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// achievementIcon may be null. Default will be used.
        /// </summary>
        /// <param name="achievementIcon"></param>
        /// <param name="achievementName"></param>
        public AchievementCompletedNotification(Texture2D achievementIcon, string achievementName)
        {
            _achievementIcon = achievementIcon;
            _achievementName = achievementName;

            ClipsBounds = false;
            // TODO: set size, or make sure it is set
            // TODO: use override RecalculateLayout() to calculate properties that rely on this.Size

            SetupShineTweener();
            SetupAchievementIconTweener();
            SetupFadeOutTweener();
        }

        private void SetupShineTweener()
        {
            Tween rotation = GameService.Animation.Tweener.Tween(this, new { _shineRotation = SHINE_ROTATION_TARGET }, SHINE_ROTATION_DURATION)
                .Rotation(Tween.RotationUnit.Radians)
                .Ease(Ease.CubeOut);

            Tween opacity = GameService.Animation.Tweener.Tween(this, new { _shineOpacity = SHINE_OPACITY_TARGET }, SHINE_OPACITY_DURATION);

            Tween scale = GameService.Animation.Tweener.Tween(this, new { _shineScale = SHINE_SCALE_TARGET }, SHINE_SCALE_DURATION);
        }

        private void SetupAchievementIconTweener()
        {
            Tween opacity = GameService.Animation.Tweener.Tween(this, new { _iconOpacity = ICON_OPACITY_TARGET }, ICON_OPACITY_DURATION, ICON_OPACITY_DELAY);

            Tween scale = GameService.Animation.Tweener.Tween(this, new { _iconScale = ICON_SCALE_TARGET }, ICON_SCALE_DURATION)
                .Ease(Ease.BackOut);
        }

        private void SetupFadeOutTweener()
        {
            Tween totalOpacity = GameService.Animation.Tweener.Tween(this, new { Opacity = 0.0f }, FADEOUT_DURATION, FADEOUT_DELAY);
            totalOpacity.OnComplete(OnLifetimeEnd);
        }

        public override void DoUpdate(GameTime gameTime)
        {
            bool shouldBeVisible =
                GameService.GameIntegration.Gw2Instance.Gw2IsRunning &&
                GameService.GameIntegration.Gw2Instance.IsInGame &&
                GameService.Gw2Mumble.IsAvailable &&
                !GameService.Gw2Mumble.UI.IsMapOpen;

            if (!Visible && shouldBeVisible)
                Show();
            else if (Visible && !shouldBeVisible)
                Hide();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                this,
                TextureManager.Notification.Background,
                bounds,
                Color.White * BackgroundOpacity * 0.8f
            );

            spriteBatch.DrawStringOnCtrl(
                this,
                $"{_achievementName}\nCompleted",
                GameService.Content.DefaultFont14,
                TextBounds,
                Color.White * BackgroundOpacity,
                verticalAlignment: VerticalAlignment.Top
            );

            spriteBatch.DrawOnCtrl(
                this,
                TextureManager.Notification.Shine,
                ShineBounds.ScaleBy(_shineScale).OffsetBy(ShineBounds.Width / 2, ShineBounds.Height / 2),
                null,
                Color.White * _shineOpacity,
                -_shineRotation,
               TextureManager.Notification.Shine.Bounds.Size.ToVector2() / 2
            );

            spriteBatch.DrawOnCtrl(
                this,
                TextureManager.Notification.Shine2,
                ShineBounds.ScaleBy(_shineScale).OffsetBy(ShineBounds.Width / 2, ShineBounds.Height / 2),
                null,
                Color.White * _shineOpacity,
                _shineRotation,
               TextureManager.Notification.Shine2.Bounds.Size.ToVector2() / 2
            );

            spriteBatch.DrawOnCtrl(
                this,
                _achievementIcon ?? TextureManager.Notification.DefaultAchievement,
                IconBounds.ScaleBy(_iconScale).CenterAround(IconCenter.ToPoint()),
                null,
                Color.White * _iconOpacity
            );
        }
    }

}
