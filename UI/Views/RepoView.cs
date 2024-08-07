using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.UI.Presenters;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Views
{
    // heavily inspired by https://github.com/blish-hud/Pathing/blob/main/UI/Views/PackRepoView.cs
    public class RepoView : View<RepoPresenter>
    {
        private const int PADDING_RIGHT = 40;
        private const int PADDING_BOTTOM = 40;

        private Container _parent;

        private FlowPanel _repoFlowPanel;
        private TextBox _searchBox;

        private string _searchText;

        public RepoView() { /** NOOP **/ }

        public RepoView(AchievementPackRepo repo) : this()
        {
            this.WithPresenter(new RepoPresenter(this, repo));
        }

        protected override void Build(Container buildPanel)
        {
            _parent = buildPanel;
            _parent.Resized += OnParentResized;

            // TODO: localize
            _searchBox = new TextBox()
            {
                PlaceholderText = "Search achievement packs...",
                Parent = buildPanel,
                Location = new Point(20, 10),
                Width = buildPanel.ContentRegion.Width - PADDING_RIGHT,
            };

            _searchBox.TextChanged += OnSearchBoxTextChanged;

            _repoFlowPanel = new FlowPanel()
            {
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - 12 - PADDING_BOTTOM),
                Top = _searchBox.Bottom + 12,
                CanScroll = true,
                ControlPadding = new Vector2(0, 15),
                OuterControlPadding = new Vector2(20, 5),
                Parent = buildPanel
            };
        }

        private void OnParentResized(object _, ResizedEventArgs _1)
        {
            RecalculateLayout();
        }

        private void OnSearchBoxTextChanged(object _, EventArgs _1)
        {
            _searchText = _searchBox.Text.ToLowerInvariant();

            _repoFlowPanel.FilterChildren<ViewContainer>(SearchFilter);
        }

        private bool SearchFilter(ViewContainer viewContainer)
        {
            if (viewContainer == null)
            {
                return false;
            }

            if (!(viewContainer.CurrentView is PkgView pkgView))
            {
                return false;
            }

            string title = pkgView.PkgBody?.Title ?? string.Empty;
            string description = pkgView.PkgBody?.Description ?? string.Empty;
            string tags = string.Join(", ", pkgView.Tags);

            return title.ToLowerInvariant().Contains(_searchText)
                || description.ToLowerInvariant().Contains(_searchText)
                || tags.ToLowerInvariant().Contains(_searchText);
        }

        private void RecalculateLayout()
        {
            int spaceWidth = _parent.ContentRegion.Width;
            int spaceHeight = _parent.ContentRegion.Height;

            if (_searchBox != null)
            {
                _searchBox.Width = spaceWidth - PADDING_RIGHT;
            }

            if (_repoFlowPanel != null)
            {
                _repoFlowPanel.Width = spaceWidth;
                _repoFlowPanel.Height = spaceHeight - _searchBox.Bottom - 12 - PADDING_BOTTOM;
                RecalculateContentLayout();
            }
        }
        
        private void RecalculateContentLayout()
        {
            ViewContainer[] currentContent = _repoFlowPanel.GetChildrenOfType<ViewContainer>().ToArray();

            foreach (ViewContainer viewContainer in currentContent)
            {
                viewContainer.Width = _repoFlowPanel.ContentRegion.Width - PADDING_RIGHT;
            }
        }

        public void SetContent(IEnumerable<PkgView> pkgViews)
        {
            if (_repoFlowPanel == null)
            {
                return;
            }

            ClearPanel();

            foreach (PkgView view in pkgViews)
            {
                ViewContainer viewContainer = new ViewContainer()
                {
                    Parent = _repoFlowPanel,
                    Width = _repoFlowPanel.ContentRegion.Width - PADDING_RIGHT,
                    Height = 200
                };

                viewContainer.Show(view);
            }
        }

        private void ClearPanel()
        {
            if (_repoFlowPanel == null)
            {
                return;
            }

            ViewContainer[] currentContent = _repoFlowPanel.GetChildrenOfType<ViewContainer>().ToArray();
            _repoFlowPanel.ClearChildren();

            foreach (ViewContainer viewContainer in currentContent)
            {
                viewContainer.Dispose();
            }
        }

        protected override void Unload()
        {
            if (_parent != null)
            {
                _parent.Resized -= OnParentResized;
                _parent = null;
            }

            if (_repoFlowPanel != null)
            {
                ClearPanel();
                _repoFlowPanel?.Dispose();
                _repoFlowPanel = null;
            }

            if (_searchBox != null)
            {
                _searchBox?.Dispose();
                _searchBox = null;
            }
        }
    }
}
