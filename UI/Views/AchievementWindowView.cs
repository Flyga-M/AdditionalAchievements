using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Presenters;
using AchievementLib.Pack;
using Flyga.AdditionalAchievements.UI.Models;

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

        private AchievementMenuView _menuView;
        private ViewContainer _menuViewContainer;

        private ViewContainer _contentViewContainer;

        private Stack<ViewCreationData> _history;
        private CollectionMenuData _currentCollection;

        public AchievementWindowView(AchievementHandler achievementHandler)
        {
            this.WithPresenter(new AchievementWindowPresenter(this, achievementHandler));

            _menuView = GetMenuView(achievementHandler);
            _menuView.CollectionSelected += OnCollectionSelected;

            _history = new Stack<ViewCreationData>();
        }

        private void OnCollectionSelected(object _, CollectionMenuData collection)
        {
            IView currentView = _contentViewContainer.CurrentView;
            if (currentView != null)
            {
                // TODO: maybe make an interface (or abstract Control) for views that have a back bar
                if (currentView is AchievementView achievementView)
                {
                    achievementView.Back -= OnBack;
                }
                else if (currentView is AchievementCollectionView previousCollectionView)
                {
                    previousCollectionView.AchievementSelected -= OnAchievementSelected;
                }
            }


            AchievementCollectionView collectionView = GetCollectionView(collection);
            _contentViewContainer.Show(collectionView);

            _currentCollection = collection;

            _history.Clear();
            _history.Push(new ViewCreationData<CollectionMenuData>(GetCollectionView, collection));
        }

        private void OnBack(object _, EventArgs _1) 
        {
            _history.Pop();

            IView currentView = _contentViewContainer.CurrentView;
            if (currentView != null)
            {
                // TODO: maybe make an interface (or abstract Control) for views that have a back bar
                if (currentView is AchievementView achievementView)
                {
                    achievementView.Back -= OnBack;
                }
            }

            if (!_history.Any())
            {
                _contentViewContainer.Hide();
                Logger.Error($"Unable to go back to last view, because the stack is empty. Hiding content view.");
                return;
            }

            _contentViewContainer.Show(_history.Peek().GetView());
        }

        private void OnAchievementSelected(object _, IAchievement achievement)
        {
            AchievementView achievementView = GetAchievementView(achievement);
            _contentViewContainer.Show(achievementView);

            _history.Push(new ViewCreationData<IAchievement>(GetAchievementView, achievement));
        }

        private AchievementView GetAchievementView(IAchievement achievement)
        {
            Texture2D icon = _currentCollection?.Icon;
            if (icon == null)
            {
                icon = ContentService.Textures.Error;
                Logger.Error("Unable to set icon for back bar. _currentCollection?.Icon is null. Using Placeholder instead.");
            }

            string name = _currentCollection?.Name;
            if (name == null)
            {
                name = "Error: Contact Module Author";
                Logger.Error("Unable to set name for back bar. _currentCollection?.Name is null. Using Placeholder instead.");
            }

            AchievementView achievementView = new AchievementView(achievement, new BackData(icon, name));
            achievementView.Back += OnBack;

            return achievementView;
        }

        private AchievementCollectionView GetCollectionView(CollectionMenuData collection)
        {
            AchievementCollectionView collectionView = new AchievementCollectionView(((AchievementWindowPresenter)this.Presenter).Model.GetAchievementsForCollection(collection.Id));
            
            collectionView.AchievementSelected += OnAchievementSelected;

            return collectionView;
        }

        private AchievementMenuView GetMenuView(AchievementHandler achievementHandler)
        {
            return new AchievementMenuView(achievementHandler);
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

        protected override Task<bool> Load(IProgress<string> progress)
        {
            return _menuView.DoLoad(progress);
        }

        protected override void Build(Container buildPanel)
        {
            // should be the window itself
            _parent = buildPanel;
            RecalculateLayout();

            buildPanel.Resized += OnParentResize;

            _menuViewContainer = BuildMenu(buildPanel);
            _menuViewContainer.Show(_menuView);

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
                _menuViewContainer?.Dispose();
                _menuViewContainer = null;
            }

            if (_menuView != null)
            {
                _menuView.DoUnload(); // should already be taken care of by the _menuViewContainer, but whatever
                _menuView = null;
            }

            if (_contentViewContainer != null)
            {
                // TODO: put in method instead of using multiple times
                IView currentView = _contentViewContainer.CurrentView;

                if (currentView != null)
                {
                    // TODO: maybe make an interface (or abstract Control) for views that have a back bar
                    if (currentView is AchievementView achievementView)
                    {
                        achievementView.Back -= OnBack;
                    }
                    else if (currentView is AchievementCollectionView previousCllectionView)
                    {
                        previousCllectionView.AchievementSelected -= OnAchievementSelected;
                    }
                }

                _contentViewContainer?.Dispose();
                _contentViewContainer = null;
            }
        }
    }
}
