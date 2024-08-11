using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Models;
using Flyga.AdditionalAchievements.UI.Presenters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementCollectionView : View, IViewSelection
    {
        private static Logger Logger = Logger.GetLogger<AchievementCollectionView>();

        private Container _parent;
        private FlowPanel _flowPanel;

        private string _title;
        private Texture2D _icon;

        private int _achievementWidth;

        /// <summary>
        /// The title of the collection.
        /// </summary>
        public string Title
        {
            get => _title ?? "";
            set => _title = value;
        }

        /// <summary>
        /// The icon for the collection.
        /// </summary>
        public Texture2D Icon
        {
            get
            {
                if (_icon == null || _icon.IsDisposed)
                {
                    return ContentService.Textures.Error;
                }

                return _icon;
            }
            set => _icon = value;
        }

        public event EventHandler<Func<IView>> Selected;

        private void OnAchievementSelected(AchievementSelection achievementSelection)
        {
            if (achievementSelection == null)
            {
                // TODO: log error
                return;
            }

            BackData backData = new BackData(Icon, Title);

            Func<IView> getView = () => null;
            if (achievementSelection.GetSelectedView != null)
            {
                getView = () => achievementSelection.GetSelectedView(new object[] { backData });
            }

            Selected?.Invoke(this, getView);
        }

        public AchievementCollectionView()
        { /** NOOP **/ }

        public AchievementCollectionView(IAchievementCollection collection) : this()
        {
            this.WithPresenter(new AchievementCollectionPresenter(this, collection));
        }

        private void RecalculateLayout()
        {
            int spaceWidth = _parent.ContentRegion.Width;
            int spaceHeigt = _parent.ContentRegion.Height;

            if (_flowPanel == null)
            {
                return;
            }

            _flowPanel.Width = spaceWidth;
            _flowPanel.Height = spaceHeigt;


            _achievementWidth = (int)((float)_flowPanel.Width / 2.0f) - 5;

            foreach (Control achievementSelection in _flowPanel.Children.ToArray())
            {
                achievementSelection.Width = _achievementWidth;
                achievementSelection.Height = (int)((float)_achievementWidth / (AchievementSelection.DEFAULT_WIDTH_HEIGHT_RATIO));
            }
        }

        private void OnParentResized(object _, ResizedEventArgs _1)
        {
            RecalculateLayout();
        }

        protected override void Build(Container buildPanel)
        {
            _parent = buildPanel;
            _parent.Resized += OnParentResized;

            _flowPanel = new FlowPanel()
            {
                Size = buildPanel.Size,
                Location = new Point(0, 0),
                FlowDirection = ControlFlowDirection.LeftToRight,
                Parent = buildPanel,
                CanScroll = true,
                ControlPadding = new Vector2(7, 7)
            };

            _achievementWidth = (int)((float)_flowPanel.Width / 2.0f) - 5;
        }

        public void SetContent(IEnumerable<Control> achievementSelections)
        {
            if (_flowPanel == null)
            {
                return;
            }

            ClearPanel();

            foreach (Control achievementSelection in achievementSelections)
            {
                achievementSelection.Parent = _flowPanel;
                achievementSelection.Click += OnAchievementSelectionSelected;

                float widthHeightRatio = (float)achievementSelection.Width / (float)achievementSelection.Height;

                achievementSelection.Width = _achievementWidth;
                achievementSelection.Height = (int)((float)_achievementWidth / widthHeightRatio);
            }
        }

        public void SortContent<TControl>(Comparison<TControl> comparison) where TControl : Control
        {
            if (_flowPanel == null || !_flowPanel.Children.Any())
            {
                return;
            }

            _flowPanel.SortChildren(comparison);
        }

        public void FilterContent<TControl>(Func<TControl, bool> filter) where TControl : Control
        {
            if (_flowPanel == null || !_flowPanel.Children.Any())
            {
                return;
            }

            _flowPanel.FilterChildren<TControl>(filter);
        }

        private void ClearPanel()
        {
            if (_flowPanel == null)
            {
                return;
            }

            Control[] currentContent = _flowPanel.GetChildrenOfType<Control>().ToArray();
            _flowPanel.ClearChildren();

            foreach (Control control in currentContent)
            {
                control.Click -= OnAchievementSelectionSelected;
                control.Dispose();
            }
        }

        private void OnAchievementSelectionSelected(object sender, MouseEventArgs _1)
        {
            OnAchievementSelected(sender as AchievementSelection);
        }

        protected override void Unload()
        {
            Selected = null;
            
            if (_parent != null)
            {
                _parent.Resized -= OnParentResized;
                _parent = null;
            }

            if (_flowPanel != null)
            {
                ClearPanel();
                _flowPanel.Dispose();
                _flowPanel = null;
            }

            base.Unload();
        }
    }
}
