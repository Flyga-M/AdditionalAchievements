using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Models;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Flyga.AdditionalAchievements.UI.Presenters;
using Flyga.AdditionalAchievements.Status;

namespace Flyga.AdditionalAchievements.UI.Views
{
    public class StatusView : View
    {
        private const string NO_CATEGORY = "__NONE";

        private Container _parent;
        private FlowPanel _flowPanel;

        private RelativeInt _statusWidth;
        private RelativeInt _statusHeight;

        private Dictionary<string, StatusDisplay[]> StatusesByCategory;

        public event EventHandler<IView> StatusSelected;

        private void OnStatusSelected(IView detailView)
        {
            StatusSelected?.Invoke(this, detailView);
        }

        public StatusView()
        {
            StatusesByCategory = new Dictionary<string, StatusDisplay[]>();

            _statusWidth = new RelativeInt(0.48f, () => _parent.ContentRegion.Width);
            _statusHeight = new RelativeInt(0.284f, () => _statusWidth);
        }

        public StatusView(StatusManager manager) : this()
        {
            WithPresenter(new StatusPresenter(this, manager));
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

            _statusWidth.Update();
            _statusHeight.Update();

            foreach (Control statusDisplay in _flowPanel.Children.ToArray())
            {
                statusDisplay.Width = _statusWidth;
                statusDisplay.Height = _statusHeight;
            }
        }

        /// <remarks>
        /// Disposes the previously set <paramref name="statusesByCategory"/>.
        /// </remarks>
        public void SetStatusesByCategory(Dictionary<string, StatusDisplay[]> statusesByCategory)
        {
            if (_flowPanel != null)
            {
                _flowPanel.Hide();
            }
            
            ClearFlowPanel();

            if (StatusesByCategory != null)
            {
                foreach(Control statusDisplay in StatusesByCategory.Values.SelectMany(status => status))
                {
                    statusDisplay.Dispose();
                }

                StatusesByCategory.Clear();
            }

            StatusesByCategory = statusesByCategory ?? new Dictionary<string, StatusDisplay[]>();

            Rebuild();

            if (_flowPanel != null)
            {
                _flowPanel.Show();
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

            Rebuild();
        }

        private void Rebuild()
        {
            if (_flowPanel == null)
            {
                // TODO: log error
                return;
            }

            ClearFlowPanel();

            if (StatusesByCategory == null)
            {
                // TODO: log warning
                return;
            }

            foreach (string category in StatusesByCategory.Keys)
            {
                // TODO: create headline

                foreach(StatusDisplay statusDisplay in StatusesByCategory[category])
                {
                    statusDisplay.Parent = _flowPanel;
                    statusDisplay.Width = _statusWidth;
                    statusDisplay.Height = _statusHeight;

                    statusDisplay.Selected += OnDisplaySelected;
                }
            }
        }

        private void ClearFlowPanel()
        {
            if (_flowPanel == null || !_flowPanel.Children.Any())
            {
                return;
            }

            foreach (StatusDisplay child in _flowPanel.GetChildrenOfType<StatusDisplay>().ToArray())
            {
                child.Selected -= OnDisplaySelected;
                child.Parent = null;
            }
        }

        private void OnDisplaySelected(object _, IView detailView)
        {
            OnStatusSelected(detailView);
        }

        /// <remarks>
        /// Will dispose the current <see cref="StatusDisplay"/>s that were last supplied 
        /// via <see cref="SetStatusesByCategory(Dictionary{string, StatusDisplay[]})"/>.
        /// </remarks>
        protected override void Unload()
        {
            StatusSelected = null;

            if (_parent != null)
            {
                _parent.Resized -= OnParentResized;
                _parent = null;
            }

            if (_flowPanel != null)
            {
                ClearFlowPanel();

                _flowPanel.Dispose();
            }

            if (StatusesByCategory != null)
            {
                SetStatusesByCategory(null); // disposes the StatusDisplays in StatusesByCategory
                StatusesByCategory = null;
            }

            base.Unload();
        }
    }
}
