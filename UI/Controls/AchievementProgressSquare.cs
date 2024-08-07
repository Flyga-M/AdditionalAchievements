using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Flyga.AdditionalAchievements.Textures;
using Flyga.AdditionalAchievements.Textures.Colors;
using Flyga.AdditionalAchievements.UI.Controller;
using Flyga.AdditionalAchievements.UI.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// Attempts to recreate the progress square that is shown for an achievement in the collection view and when viewing 
    /// a single achievement.
    /// </summary>
    public class AchievementProgressSquare : Control<AchievementProgressSquareController>
    {
        private const int DEFAULT_WIDTH = 162;
        private const int DEFAULT_HEIGHT = 162;

        private int _maxFill;
        private int _currentFill;

        private Texture2D _icon;

        #region calculated fields

        // fields dependent on this.Height / this.Width
        private Point _center;
        private int _iconPadding;
        private Point _iconPosition;
        private RelativeInt _iconHeight;
        private int _lockPadding;
        private Point _lockPosition;
        private RelativeInt _lockHeight;
        private RelativeInt _tierHeight;
        private RelativeInt _tierFontHeight;
        private RelativeBitmapFont _tierFont;
        private RelativeInt _fillFractionHeight;
        private RelativeInt _fillFractionPadding;
        private RelativeBitmapFont _fillFractionFont;

        // calculated fields on achievement status change
        private float _currentFillPercent;
        private int _currentFillHeight;

        // animated fields (calculation)
        private float _currentIconFillPercent;
        private float _animatedFillPercent;
        private Rectangle _iconGrayDestination;
        private Rectangle _iconGraySource;
        private Rectangle _iconColorDestination;
        private Rectangle _iconColorSource;
        private Rectangle _crestSource;

        // directly animated fields
        private int _animatedCurrentFillHeight;
        private Glide.Tween _animatedFill;

        #endregion

        /// <summary>
        /// Animated or direct current fill percent dependent on <see cref="_animateFill"/>.
        /// </summary>
        private float FillPercent
        {
            get
            {
                if (AnimateFill)
                {
                    return _animatedFillPercent;
                }
                return _currentFillPercent;
            }
        }

        /// <summary>
        /// Animated or direct current fill height dependent on <see cref="_animateFill"/>.
        /// </summary>
        private int FillHeight
        {
            get
            {
                if (AnimateFill)
                {
                    //if (_animatedFill != null)
                    //{
                    //    if (_animatedFill.Completion >= 1.0f)
                    //    {
                    //        return _currentFillHeight;
                    //    }
                    //}
                    return _animatedCurrentFillHeight;
                }

                return _currentFillHeight;
            }
        }

        /// <summary>
        /// The icon that is displayed for the achievement.
        /// </summary>
        /// <remarks>
        /// Will NOT be disposed when the <see cref="AchievementProgressSquare"/> is disposed.
        /// </remarks>
        public Texture2D Icon
        {
            get => _icon ?? TextureManager.Notification.DefaultAchievement;
            set => _icon = value;
        }

        /// <summary>
        /// Determines whether a vignette is drawn on top of the <see cref="AchievementProgressSquare"/>.
        /// </summary>
        /// <remarks>
        /// Usually NOT visible on completed achievements.
        /// </remarks>
        public bool ShowVignette { get; set; }

        /// <summary>
        /// Determines whether the current tier is displayed on top of the <see cref="AchievementProgressSquare"/>.
        /// </summary>
        /// <remarks>
        /// Usually NOT visible on completed achievements. Usually NOT visible on achievements that only have one tier.
        /// </remarks>
        public bool ShowTier { get; set; }

        /// <summary>
        /// Determines whether the current completion is displayed with an overlay of color, a (partially) greyed out 
        /// part of the <see cref="Icon"/> and a fill line.
        /// </summary>
        /// <remarks>
        /// Usually NOT visible on completed achievements.
        /// </remarks>
        public bool ShowFill { get; set; }

        /// <summary>
        /// Determines whether the current fill progress is displayed at the bottom. e.g. 3/7.
        /// </summary>
        /// <remarks>
        /// Usually NOT visible on completed achievements.
        /// </remarks>
        public bool ShowFillFraction { get; set; }

        /// <summary>
        /// Determines whether the <see cref="AchievementProgressSquare"/> should be displayed with a 
        /// tinted background.
        /// </summary>
        /// <remarks>
        /// Usually visible on completed achievements.
        /// </remarks>
        public bool ShowBackgroundTint { get; set; }

        /// <summary>
        /// Determines whether the fill overlay should be animated.
        /// </summary>
        public bool AnimateFill { get; set; }

        /// <summary>
        /// The color that is used for the fill overlay, if <see cref="ShowFill"/> is <see langword="true"/>.
        /// </summary>
        public Color FillColor { get; set; } = ColorManager.AchievementFallbackColor;

        /// <summary>
        /// The maximum fill, that the achievement can reach.
        /// </summary>
        /// <remarks>
        /// Usually corresponds with the maximum amount of objectives (for the current tier).
        /// </remarks>
        public int MaxFill
        {
            get => _maxFill;
            set
            {
                int oldValue = _maxFill;
                _maxFill = value;
                if (oldValue != value)
                {
                    OnFillChanged();
                }
            }
        }

        /// <summary>
        /// The current fill.
        /// </summary>
        /// <remarks>
        /// Usually corresponds with the current amount of completed objectives.
        /// </remarks>
        public int CurrentFill
        {
            get => _currentFill;
            set
            {
                int oldValue = _currentFill;
                _currentFill = value;
                if (oldValue != value)
                {
                    OnFillChanged();
                }
            }
        }

        public int CurrentTier { get; set; }

        public bool IsLocked { get; set; }

        public AchievementProgressSquare()
        {
            _iconHeight = new RelativeInt(0.65f, () => this.Height); //(int)Math.Floor(((float)this.Height * 0.75f));
            _lockHeight = new RelativeInt(0.8f, () => this.Height);

            _tierHeight = new RelativeInt(0.183f, () => this.Height); //(int)(this.Height * 0.183);
            _tierFontHeight = new RelativeInt(0.85f, () => this._tierHeight);

            _tierFont = new RelativeBitmapFont(_tierFontHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular); //FontManager.GetFontFromHeight((int)(_tierHeight * 0.85f), ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            _fillFractionHeight = new RelativeInt(0.20f, () => this.Height); //(int)(this.Height * 0.15f);
            _fillFractionPadding = new RelativeInt(0.05f, () => this.Height); //(int)(this.Height * 0.05f);

            _fillFractionFont = new RelativeBitmapFont(_fillFractionHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular); //FontManager.GetFontFromHeight(_fillFractionHeight, ContentService.FontFace.Menomonia, ContentService.FontStyle.Regular);

            Width = DEFAULT_WIDTH;
            Height = DEFAULT_HEIGHT;
        }

        public AchievementProgressSquare(IAchievement achievement, bool alwaysHideFillFraction, bool showFillForCurrentTier = false) : this()
        {
            this.WithController(
                new AchievementProgressSquareController(this, achievement)
                {
                    ShowFillForCurrentTier = showFillForCurrentTier,
                    AlwaysHideFillFraction = alwaysHideFillFraction
                }
            );
        }

        public static AchievementProgressSquare ForCompleted()
        {
            return new AchievementProgressSquare()
            {
                ShowVignette = false,
                ShowTier = false,
                ShowFill = false,
                ShowFillFraction = false,
                AnimateFill = false,
                ShowBackgroundTint = false,
            };
        }

        public static AchievementProgressSquare ForUncompleted()
        {
            return new AchievementProgressSquare()
            {
                ShowVignette = true,
                ShowTier = true,
                ShowFill = true,
                ShowFillFraction = true,
                AnimateFill = true,
                ShowBackgroundTint = true,
            };
        }

        private void OnFillChanged()
        {
            RecalculateFill();
        }

        public override void RecalculateLayout()
        {
            _center = new Point(this.Width / 2, this.Height / 2);

            _iconHeight.Update();
            _iconPadding = (int)Math.Floor(((float)(this.Height - _iconHeight) / 2.0f));
            _iconPosition = new Point(_center.X - _iconHeight / 2, _center.Y - _iconHeight / 2);

            _lockHeight.Update();
            _lockPadding = (int)Math.Floor(((float)(this.Height - _lockHeight) / 2.0f));
            _lockPosition = new Point(_center.X - _lockHeight / 2, _center.Y - _lockHeight / 2);

            _tierHeight.Update();
            _tierFont.Update();

            _fillFractionHeight.Update();
            _fillFractionPadding.Update();

            _fillFractionFont.Update();

            // reset fillHeight after the layout was recalculated
            // otherwise the animation in RecalculateFill() won't work properly
            // -> the fill animation always resets when resizing

            _currentFillHeight = 0;
            // dependent on this.Height
            RecalculateFill();

            // some of those fields also change, when the above value changes
            OnAnimationUpdate();
        }

        /// <summary>
        /// Recalculate fill, after achievement updates.
        /// </summary>
        private void RecalculateFill()
        {
            int oldFillHeight = _currentFillHeight;
            _currentFillPercent = RecalculateFillPercent();
            _currentFillHeight = RecalculateFillHeight();
            //_animatedFillPercent = _currentFillPercent;

            if (oldFillHeight != _currentFillHeight)
            {
                _animatedFill?.Cancel();
                _animatedFill = null;

                _animatedCurrentFillHeight = oldFillHeight;

                _animatedFill = Animation.Tweener.Tween(this, new { _animatedCurrentFillHeight = _currentFillHeight }, 0.65f)
                    .Ease(Glide.Ease.QuintIn);

                _animatedFill.OnUpdate(OnAnimationUpdate);
            }
        }

        private void OnAnimationUpdate()
        {
            _animatedFillPercent = (float)FillHeight / (float)this.Height;

            _currentIconFillPercent = RecalculateIconFillPercent();

            _iconGrayDestination = new Rectangle(_iconPosition.X, _iconPosition.Y, _iconHeight, (int)((float)_iconHeight * (1.0f - _currentIconFillPercent)));
            _iconGraySource = new Rectangle(0, 0, Icon.Width, (int)((float)Icon.Height * (1.0f - _currentIconFillPercent)));
            _iconColorDestination = new Rectangle(_iconPosition.X, _iconGrayDestination.Bottom, _iconHeight, _iconHeight - _iconGrayDestination.Height);
            _iconColorSource = new Rectangle(0, _iconGraySource.Bottom, Icon.Width, Icon.Height - _iconGraySource.Height);

            _crestSource = new Rectangle(0, 0, TextureManager.Display.Progress.FillCrest.Width, (int)(TextureManager.Display.Progress.FillCrest.Height * _currentIconFillPercent));
        }

        private float RecalculateFillPercent()
        {
            return (float)CurrentFill / (float)MaxFill;
            
            //if (_achievement.IsFulfilled)
            //{
            //    return 1;
            //}

            //if (_achievement.CurrentObjectives == 0)
            //{
            //    return 0;
            //}

            //if (!ShowFillForCurrentTier)
            //{
            //    return (float)_achievement.CurrentObjectives / (float)_achievement.Tiers.Last();
            //}

            //int currentTier = _achievement.CurrentTier;
            //int maxObjectivesForCurrentTier = _achievement.Tiers.ElementAt(currentTier - 1);

            //return (float)_achievement.CurrentObjectives / (float)maxObjectivesForCurrentTier;
        }

        private int RecalculateFillHeight()
        {
            if (_currentFillPercent >= 1.0f)
            {
                return this.Height;
            }

            if (_currentFillPercent <= 0.0f)
            {
                return 0;
            }

            float result = (float)this.Height * _currentFillPercent;

            return (int)Math.Floor(result);
        }

        private float RecalculateIconFillPercent()
        {
            if (FillHeight < _iconPadding)
            {
                return 0.0f;
            }

            if (FillHeight > _iconPadding + _iconHeight)
            {
                return 1.0f;
            }

            return (float)(FillHeight - _iconPadding) / (float)_iconHeight;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // tinted background
            if (ShowBackgroundTint)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.25f);
            }

            PaintIcon(spriteBatch);
            PaintFill(spriteBatch);
            PaintTier(spriteBatch);
            PaintVignette(spriteBatch);
        }

        private void PaintIcon(SpriteBatch spriteBatch)
        {   
            // only the lock, if IsLocked is true
            if (IsLocked)
            {
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.Progress.Lock,
                    new Rectangle(_lockPosition, new Point(_lockHeight, _lockHeight)));
                return;
            }

            // full icon, if partial fill should not be visible
            if (!ShowFill)
            {
                spriteBatch.DrawOnCtrl(this,
                    Icon,
                    new Rectangle(_iconPosition, new Point(_iconHeight, _iconHeight)));
                return;
            }

            // grayed out part of the icon
            if (_currentIconFillPercent < 1)
            {
                spriteBatch.DrawOnCtrl(this,
                    Icon,
                    _iconGrayDestination,
                    _iconGraySource,
                    Color.DarkGray * 0.4f);
            }

            // colored part of the icon
            if (_currentIconFillPercent > 0)
            {
                spriteBatch.DrawOnCtrl(this,
                    Icon,
                    _iconColorDestination,
                    _iconColorSource
                    );
            }
        }

        private void PaintFill(SpriteBatch spriteBatch)
        {
            if (!ShowFill || IsLocked)
            {
                return;
            }

            // color
            spriteBatch.DrawOnCtrl(this,
                ContentService.Textures.Pixel,
                new Rectangle(0, this.Height - FillHeight, this.Width, FillHeight),
                FillColor * 0.3f);

            // crest
            if (FillPercent < 1)
                spriteBatch.DrawOnCtrl(this,
                    TextureManager.Display.Progress.FillCrest,
                    new Rectangle(0, this.Height - FillHeight, this.Width, FillHeight),
                    _crestSource
                );

            // fill fraction
            if (ShowFillFraction)
            {
                spriteBatch.DrawStringOnCtrl(this,
                    $"{CurrentFill}/{MaxFill}",
                    _fillFractionFont,
                    new Rectangle(0, this.Height - _fillFractionHeight - _fillFractionPadding, this.Width, _fillFractionHeight),
                    Color.White,
                    false,
                    true,
                    1,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Middle);
            }
        }

        private void PaintTier(SpriteBatch spriteBatch)
        {
            if (!ShowTier || IsLocked)
            {
                return;
            }

            // background
            spriteBatch.DrawOnCtrl(this,
                TextureManager.Display.Progress.TierBackground,
                new Rectangle(0, 0, this.Width, this.Height),
                Color.White * 0.8f);

            // text
            spriteBatch.DrawStringOnCtrl(this,
                    RomanNumeralUtil.ToRomanNumeral(CurrentTier),
                    _tierFont,
                    new Rectangle(0, 0, this.Width, _tierHeight),
                    Color.White,
                    false,
                    true,
                    1,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Middle);
        }

        private void PaintVignette(SpriteBatch spriteBatch)
        {
            if (!ShowVignette)
            {
                return;
            }

            spriteBatch.DrawOnCtrl(this,
                TextureManager.Display.Progress.Vignette,
                new Rectangle(0, 0, this.Width, this.Height));
        }

        protected override void DisposeControl()
        {
            _animatedFill?.Cancel();
            _animatedFill = null;

            base.DisposeControl();
        }
    }
}
