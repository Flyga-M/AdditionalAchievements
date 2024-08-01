using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Resources;
using Microsoft.Xna.Framework;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Presenters;
using Flyga.AdditionalAchievements.UI.Models;
using Flyga.AdditionalAchievements.UI.Controls;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class AchievementMenuView : View
    {
        private static Logger Logger = Logger.GetLogger<AchievementMenuView>();

        private const int SEARCH_BAR_GAP = 7;

        private Container _parent;

        public Dictionary<string, CollectionMenuData[]> Categories;

        private TextBox _searchBox;
        private Panel _menuPanel;

        public event EventHandler<CollectionMenuData> CollectionSelected;

        private void OnCollectionSelected(CollectionMenuData collectionData)
        {
            CollectionSelected?.Invoke(this, collectionData);
        }

        public AchievementMenuView(AchievementHandler achievementHandler)
        {
            this.WithPresenter(new AchievementMenuPresenter(this, achievementHandler));
        }

        protected override void Build(Container buildPanel)
        {
            _parent = buildPanel;

            _searchBox = new TextBox()
            {
                PlaceholderText = Strings.PlaceholderSearch,
                Size = new Point(_parent.Width - 30, 30),
                //Font = GameService.Content.DefaultFont16,
                Location = new Point(0, 0),
                Parent = _parent
            };
            try
            {
                BuildMenuPanel();
            }
            catch (Exception ex) { Logger.Warn($"Ex: {ex}"); }
        }

        internal void BuildMenuPanel()
        {
            if (_parent == null || _searchBox == null)
            {
                return;
            }
            
            if (_menuPanel != null)
            {
                UnsubscribeFromItemSelectedEvent();
                _menuPanel?.Dispose();
            }

            _menuPanel = new Panel()
            {
                Title = Strings.Categories,
                ShowBorder = false,
                Size = new Point(_parent.Width, _parent.Height - (_searchBox.Height + SEARCH_BAR_GAP)),
                Location = new Point(0, _searchBox.Height + SEARCH_BAR_GAP),
                CanScroll = true,
                Parent = _parent
            };

            Menu menu = new Menu
            {
                Width = _parent.Width,
                Parent = _menuPanel
            };

            foreach (KeyValuePair<string, CollectionMenuData[]> category in Categories.ToArray())
            {
                MenuItem menuItem = menu.AddMenuItem(category.Key);
                foreach (CollectionMenuData collection in category.Value)
                {
                    MenuItemWithData<CollectionMenuData> subItem = new MenuItemWithData<CollectionMenuData>(collection.Name, collection.Icon)
                    {
                        Parent = menuItem,
                        Data = collection
                    };

                    subItem.ItemSelected += OnMenuItemSelected;
                }

                menuItem.RecalculateLayout();
            }
        }

        private void OnMenuItemSelected(object sender, ControlActivatedEventArgs eventArgs)
        {
            if (!(eventArgs.ActivatedControl is MenuItemWithData<CollectionMenuData> menuItem))
            {
                Logger.Error($"Unable to select achievement collection in {this.GetType()}. Provided control is not a {typeof(MenuItemWithData<string>)}. " +
                    $"Given Type: {eventArgs.ActivatedControl?.GetType()}");
                return;
            }

            OnCollectionSelected(menuItem.Data);
        }

        private void UnsubscribeFromItemSelectedEvent()
        {
            if (_menuPanel == null || _menuPanel.Children == null || !_menuPanel.Children.Any())
            {
                return;
            }

            IEnumerable<Menu> menues = _menuPanel.Children.Where(child => typeof(Menu).IsAssignableFrom(child.GetType())).Select(child => (Menu)child);

            foreach(Menu menu in menues)
            {
                UnsubscribeMenuFromItemSelectedEvent(menu);
            }
        }

        private void UnsubscribeMenuFromItemSelectedEvent(Menu menu)
        {
            if (menu == null || menu.Children == null || !menu.Children.Any())
            {
                return;
            }

            IEnumerable<MenuItem> menuItems = menu.Children.Where(child => typeof(MenuItem).IsAssignableFrom(child.GetType())).Select(child => (MenuItem)child);

            foreach (MenuItem menuItem in menuItems)
            {
                UnsubscribeMenuItemFromItemSelectedEvent(menuItem, OnMenuItemSelected);
            }
        }

        // TODO: should maybe be an extension method ¯\_(ツ)_/¯
        private void UnsubscribeMenuItemFromItemSelectedEvent(MenuItem menuItem, EventHandler<ControlActivatedEventArgs> action)
        {
            if (menuItem == null)
            {
                return;
            }

            menuItem.ItemSelected -= action;

            if (menuItem.Children == null || !menuItem.Children.Any())
            {
                return;
            }

            IEnumerable<MenuItem> children = menuItem.Children.Where(child => typeof(MenuItem).IsAssignableFrom(child.GetType())).Select(child => (MenuItem)child);

            foreach (MenuItem child in children)
            {
                UnsubscribeMenuItemFromItemSelectedEvent(child, OnMenuItemSelected);
            }
        }

        protected override void Unload()
        {
            CollectionSelected = null;
            
            if (_parent != null)
            {
                if (_searchBox != null)
                {
                    _parent.RemoveChild(_searchBox);
                }

                if (_menuPanel != null)
                {
                    _parent.RemoveChild(_menuPanel);
                }
            }

            if (_searchBox != null)
            {
                _searchBox?.Dispose();
                _searchBox = null;
            }

            if (_menuPanel != null)
            {
                // not sure if this is even neccessary, but i  guess it doesn't hurt
                UnsubscribeFromItemSelectedEvent();

                // will automatically dispose all children, since it's a container
                _menuPanel?.Dispose();
                _menuPanel = null;
            }
        }
    }
}
