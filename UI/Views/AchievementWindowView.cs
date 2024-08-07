using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Models;
using Flyga.AdditionalAchievements.UI.Presenters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementWindowView : View
    {
        private static Logger Logger = Logger.GetLogger<AchievementWindowView>();

        private Container _parent;

        private int _menuWidth;
        private int _menuHeight;

        private int _contentWidth;
        private int _contentHeight;

        private IViewSelection _menuView;

        private ViewContainer _menuViewContainer;

        private ViewContainer _contentViewContainer;

        private Stack<ViewCreationData> _history;

        /// <remarks>
        /// Will unload the previous <see cref="MenuView"/> when overwritten. 
        /// Will be unloaded when the <see cref="AchievementWindowView"/> is unloaded.
        /// </remarks>
        public IViewSelection MenuView
        {
            get => _menuView;
            set
            {
                if (_menuView != null)
                {
                    _menuView.Selected -= OnSubViewClearSelected;
                    _menuView.DoUnload();
                }

                _menuView = value;
                if (_menuView != null)
                {
                    _menuView.Selected += OnSubViewClearSelected;
                }

                if (_menuViewContainer != null)
                {
                    _menuViewContainer.Show(_menuView);
                }
            }
        }

        public AchievementWindowView()
        {
            _history = new Stack<ViewCreationData>();
        }

        public AchievementWindowView(AchievementHandler achievementHandler) : this()
        {
            this.WithPresenter(new AchievementWindowPresenter(this, achievementHandler));
        }

        private void OnBack(object _, EventArgs _1) 
        {
            _history.Pop();

            ClearContentViewEventListeners(_contentViewContainer.CurrentView);

            if (!_history.Any())
            {
                _contentViewContainer.Hide();
                Logger.Error($"Unable to go back to last subView, because the stack is empty. Hiding content subView.");
                return;
            }

            IView newView = _history.Peek().GetView();
            AddContentViewEventListeners(newView);
            _contentViewContainer.Show(newView);
        }

        private void OnSubViewClearSelected(object sender, Func<IView> getSubView)
        {
            _history.Clear();

            OnSubViewSelected(sender, getSubView);
        }

        private void OnSubViewSelected(object _, Func<IView> getSubView)
        {
            ClearContentViewEventListeners(_contentViewContainer.CurrentView);

            IView subView = getSubView();
            AddContentViewEventListeners(subView);
            _contentViewContainer.Show(subView);

            _history.Push(new ViewCreationData(getSubView));
        }

        private void AddContentViewEventListeners(IView subView)
        {
            if (subView == null)
            {
                return;
            }
            
            if (subView is IBack newBackableAchievementView)
            {
                newBackableAchievementView.Back += OnBack;
            }
            if (subView is IViewSelection newViewSelection)
            {
                newViewSelection.Selected += OnSubViewSelected;
            }
        }

        private void ClearContentViewEventListeners(IView subView)
        {
            if (subView == null)
            {
                return;
            }

            if (subView is IBack newBackableAchievementView)
            {
                newBackableAchievementView.Back -= OnBack;
            }
            if (subView is IViewSelection newViewSelection)
            {
                newViewSelection.Selected -= OnSubViewSelected;
            }
        }

        public void RecalculateLayout()
        {
            int spaceWidth = _parent.ContentRegion.Width;
            int spaceHeigt = _parent.ContentRegion.Height;

            _menuWidth = (int)((float)spaceWidth / 3.5f);
            _menuHeight = spaceHeigt;
            
            if (_menuViewContainer != null)
            {
                _menuViewContainer.Width = _menuWidth;
                _menuViewContainer.Height = _menuHeight;
            }

            _contentWidth = spaceWidth - _menuWidth;
            _contentHeight = spaceHeigt;

            if (_contentViewContainer != null)
            {
                _contentViewContainer.Width = _contentWidth;
                _contentViewContainer.Height = _contentHeight;
                _contentViewContainer.Location = new Point(_menuWidth, 0);
            }
        }

        private void OnParentResize(object _, ResizedEventArgs arguments)
        {
            RecalculateLayout();
        }

        protected override void Build(Container buildPanel)
        {
            // should be the window itself
            _parent = buildPanel;
            RecalculateLayout();

            buildPanel.Resized += OnParentResize;

            _menuViewContainer = BuildMenu(buildPanel);
            if (MenuView != null)
            {
                _menuViewContainer.Show(MenuView);
            }

            _contentViewContainer = BuildContent(buildPanel);
        }

        private ViewContainer BuildMenu(Container parentContainer)
        {
            ViewContainer menuViewContainer = new ViewContainer()
            {
                ShowBorder = false,
                Size = new Point(_menuWidth, _menuHeight),
                Location = new Point(0, 0),
                Parent = parentContainer,
                Visible = true
            };

            return menuViewContainer;
        }

        private ViewContainer BuildContent(Container parentContainer)
        {
            ViewContainer contentViewContainer = new ViewContainer()
            {
                ShowBorder = true,
                Size = new Point(_contentWidth, _contentHeight),
                Location = new Point(_menuWidth, 0),
                Parent = parentContainer,
                Visible = true
            };

            return contentViewContainer;
        }

        protected override void Unload()
        {
            if (_parent != null)
            {
                _parent.Resized -= OnParentResize;
                _parent = null;
            }

            if (_menuViewContainer != null)
            {
                // will unload the view and unsubscribe from events
                MenuView = null;

                _menuViewContainer?.Dispose();
                _menuViewContainer = null;
            }

            if (_contentViewContainer != null)
            {
                IView currentView = _contentViewContainer.CurrentView;

                ClearContentViewEventListeners(currentView);

                _contentViewContainer?.Dispose();
                _contentViewContainer = null;
            }
        }
    }
}
