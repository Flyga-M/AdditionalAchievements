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
    public class AchievementMenuView : View, IViewSelection
    {
        private static Logger Logger = Logger.GetLogger<AchievementMenuView>();

        private const int SEARCH_BAR_GAP = 7;

        private Container _parent;

        private TextBox _searchBox;
        private Panel _menuPanel;
        private Menu _menu;

        public event EventHandler<Func<IView>> Selected;

        public AchievementMenuView(AchievementHandler achievementHandler)
        {
            this.WithPresenter(new AchievementMenuPresenter(this, achievementHandler));
        }

        protected override void Build(Container buildPanel)
        {
            _parent = buildPanel;

            int spaceWidth = buildPanel.ContentRegion.Width;
            int spaceHeight = buildPanel.ContentRegion.Height;

            _searchBox = new TextBox()
            {
                PlaceholderText = Strings.PlaceholderSearch,
                Size = new Point(spaceHeight - 30, 30),
                //Font = GameService.Content.DefaultFont16,
                Location = new Point(0, 0),
                Parent = buildPanel
            };

            _menuPanel = new Panel()
            {
                Title = Strings.Categories,
                ShowBorder = false,
                Size = new Point(spaceWidth, spaceHeight - (_searchBox.Height + SEARCH_BAR_GAP)),
                Location = new Point(0, _searchBox.Height + SEARCH_BAR_GAP),
                CanScroll = true,
                Parent = buildPanel
            };

            _menu = new Menu
            {
                Width = spaceWidth,
                Parent = _menuPanel,
                CanSelect = true
            };

            _menu.ItemSelected += OnMenuItemSelected;
        }

        public void SetContent(IEnumerable<MenuItem> menuItems)
        {
            if (_menu == null)
            {
                return;
            }

            ClearMenu();

            foreach (MenuItem menuItem in menuItems)
            {
                menuItem.Parent = _menu;
            }
        }

        private void ClearMenu()
        {
            if (_menu == null)
            {
                return;
            }

            MenuItem[] currentContent = _menu.GetChildrenOfType<MenuItem>().ToArray();
            _menu.ClearChildren();

            foreach (MenuItem menuItem in currentContent)
            {
                menuItem.Dispose();
            }
        }

        private void OnMenuItemSelected(object sender, ControlActivatedEventArgs eventArgs)
        {
            if (!(eventArgs.ActivatedControl is MenuItemWithData<Func<IView>> menuItem))
            {
                // ignore clicked categories
                return;
            }

            Selected?.Invoke(this, menuItem.Data);
        }

        protected override void Unload()
        {
            Selected = null;

            _parent = null;

            if (_searchBox != null)
            {
                _searchBox.Parent = null;
                _searchBox?.Dispose();
                _searchBox = null;
            }

            if (_menu != null)
            {
                ClearMenu();
                _menu.ItemSelected -= OnMenuItemSelected;
                _menu.Parent = null;
                _menu.Dispose();
                _menu = null;
            }

            if (_menuPanel != null)
            {
                _menuPanel.Parent = null;
                _menuPanel?.Dispose();
                _menuPanel = null;
            }
        }
    }
}
