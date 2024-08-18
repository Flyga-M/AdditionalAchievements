using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Flurl;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.Resources;
using Flyga.AdditionalAchievements.UI.Controls;
using Flyga.AdditionalAchievements.UI.Presenters;
using Microsoft.Xna.Framework;
using System;

namespace Flyga.AdditionalAchievements.UI.Views
{
    // heavily inspired by https://github.com/blish-hud/Pathing/blob/main/UI/Controls/MarkerPackHero.cs
    public class PkgView : View<PkgPresenter>
    {
        private const int DEFAULT_WIDTH = 500;
        private const int DEFAULT_HEIGHT = 170;

        private const int EDGE_PADDING = 20;

        private const int BOTTOM_HEIGHT = 40;

        private Container _parent;

        private bool _canDownload;
        private bool _canUpdate;
        private bool _canDelete;
        private bool _canEnable;
        private bool _lockAllButtons;

        private bool _isEnabled;

        private string _downloadTooltip;
        private string _deleteTooltip;

        private string _packInfoUrl;

        private string[] _tags;

        private PkgBody _pkgBody;
        

        private StandardButton _downloadButton;
        private StandardButton _infoButton;
        private StandardButton _deleteButton;
        private StandardButton _enableButton;
        private Panel _bottomPanel;
        private Label _tagsLabel;

        public event EventHandler DownloadClicked;
        public event EventHandler InfoClicked;
        public event EventHandler DeleteClicked;
        public event EventHandler EnableClicked;

        #region calculated fields

        private int _bottomPartTop;
        private int _buttonsLeft;

        #endregion

        /// <remarks>
        /// Will dispose the previous <see cref="PkgBody"/>, if a new one is set. 
        /// Will be disposed when the <see cref="PkgView"/> is unloaded.
        /// </remarks>
        public PkgBody PkgBody
        {
            get => _pkgBody;
            set
            {
                if (_pkgBody == value)
                {
                    return;
                }

                if (_pkgBody != null)
                {
                    _pkgBody?.Dispose();
                }

                if (value != null && _parent != null)
                {
                    value.Parent = _parent;
                }

                _pkgBody = value;
                UpdatePkgBody();
            }
        }

        public bool CanDownload
        {
            get => _canDownload;
            set
            {
                bool oldValue = _canDownload;
                _canDownload = value;

                if (oldValue != value)
                {
                    UpdateButtons();
                }
            }
        }

        public bool CanUpdate
        {
            get => _canUpdate;
            set
            {
                bool oldValue = _canUpdate;
                _canUpdate = value;

                if (oldValue != value)
                {
                    UpdateButtons();
                }
            }
        }

        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                bool oldValue = _canDelete;
                _canDelete = value;

                if (oldValue != value)
                {
                    UpdateButtons();
                }
            }
        }

        public bool CanEnable
        {
            get => _canEnable;
            set
            {
                bool oldValue = _canEnable;
                _canEnable = value;

                if (oldValue != value)
                {
                    UpdateButtons();
                }
            }
        }

        public bool LockAllButtons
        {
            get => _lockAllButtons;
            set
            {
                bool oldValue = _lockAllButtons;
                _lockAllButtons = value;

                if (oldValue != value)
                {
                    UpdateButtons();
                }
            }
        }

        /// <summary>
        /// Determines whether the enable button shows the "Enable" or "Disable" text.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                bool oldValue = _isEnabled;
                _isEnabled = value;

                if (oldValue != value)
                {
                    UpdateButtons();
                }
            }
        }

        /// <summary>
        /// Overrides the tooltip on the download button if it's disabled.
        /// </summary>
        public string DownloadTooltip
        {
            get => _downloadTooltip;
            set
            {
                _downloadTooltip = value;
                if (_downloadButton != null)
                {
                    _downloadButton.BasicTooltipText = value;
                }
            }
        }

        /// <summary>
        /// Overrides the tooltip on the delete button if it's disabled. 
        /// [Currently useless, since it's also hidden]
        /// </summary>
        public string DeleteTooltip
        {
            get => _deleteTooltip;
            set
            {
                _deleteTooltip = value;
                if (_deleteButton != null)
                {
                    _deleteButton.BasicTooltipText = value;
                }
            }
        }

        public string PackInfoUrl
        {
            get => _packInfoUrl;
            set
            {
                string oldValue = _packInfoUrl;
                _packInfoUrl = value;

                if (oldValue != value)
                {
                    UpdateButtons();
                }
            }
        }

        public string[] Tags
        {
            get => _tags ?? Array.Empty<string>();
            set
            {
                _tags = value;
                UpdateTags();
            }
        }

        public PkgView()
        { /** NOOP **/ }

        public PkgView(AchievementPackPkg pkg) : this()
        {
            this.WithPresenter(new PkgPresenter(this, pkg));
        }

        protected override void Build(Container buildPanel)
        {
            _parent = buildPanel;
            _parent.Resized += OnParentResized;

            if (_pkgBody != null)
            {
                _pkgBody.Parent = buildPanel;
            }

            _bottomPanel = new Panel()
            {
                Height = BOTTOM_HEIGHT,
                Width = buildPanel.ContentRegion.Width,
                ShowTint = true,
                BackgroundColor = Color.Black * 0.5f,
                Parent = buildPanel
            };

            _tagsLabel = new Label()
            {
                Height = _bottomPanel.Height,
                Left = 20,
                Width = _bottomPanel.Width - 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Middle,
                Parent = _bottomPanel,
                TextColor = Control.StandardColors.Default,
                Text = string.Join(", ", Tags)
            };

            // TODO: maybe use a flowpanel for all the buttons
            _downloadButton = new StandardButton()
            {
                Text = Strings.Download,
                Width = 90,
                Parent = buildPanel,
                Enabled = _canDownload && !_lockAllButtons
            };

            _infoButton = new StandardButton()
            {
                Text = Strings.Info,
                Width = 90,
                Visible = !string.IsNullOrWhiteSpace(PackInfoUrl),
                BasicTooltipText = PackInfoUrl,
                Parent = buildPanel,
                Enabled =  !_lockAllButtons
            };

            _deleteButton = new StandardButton()
            {
                Text = Strings.Delete,
                Width = 90,
                Parent = buildPanel,
                Enabled = _canDelete && !_lockAllButtons,
                Visible = _canDelete
            };

            _enableButton = new StandardButton()
            {
                Text = Strings.Enable,
                Width = 90,
                Parent = buildPanel,
                Enabled = !_lockAllButtons,
                Visible = true
            };

            _downloadButton.Click += OnDownloadButtonClick;
            _infoButton.Click += OnInfoButtonClick;
            _deleteButton.Click += OnDeleteButtonClick;
            _enableButton.Click += OnEnableButtonClick;

            UpdateButtonStates(); // button layout will be updated by recalculateLayout
            RecalculateLayout();
        }

        private void OnParentResized(object _, ResizedEventArgs _1)
        {
            RecalculateLayout();
        }

        private void OnDownloadButtonClick(object _, MouseEventArgs e)
        {
            if (CanDownload || CanUpdate)
            {
                DownloadClicked?.Invoke(this, null);
            }
        }

        private void OnInfoButtonClick(object _, MouseEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PackInfoUrl))
            {
                InfoClicked?.Invoke(this, null);
            }
        }

        private void OnDeleteButtonClick(object _, MouseEventArgs e)
        {
            if (CanDelete)
            {
                DeleteClicked?.Invoke(this, null);
            }
        }

        private void OnEnableButtonClick(object _, MouseEventArgs e)
        {
            EnableClicked?.Invoke(this, null);
        }

        private void RecalculateLayout()
        {
            int spaceWidth = DEFAULT_WIDTH;

            if (_parent != null)
            {
                spaceWidth = _parent.ContentRegion.Width;
            }

            UpdatePkgBody();

            if (_bottomPanel != null)
            {
                _bottomPanel.Top = _bottomPartTop;
                _bottomPanel.Width = spaceWidth;
            }

            UpdateButtonLayout(); // will also update tags layout
        }

        private void UpdatePkgBody()
        {
            int spaceWidth = DEFAULT_WIDTH;
            int spaceHeight = DEFAULT_HEIGHT;

            if (_parent != null)
            {
                spaceWidth = _parent.ContentRegion.Width;
                spaceHeight = _parent.ContentRegion.Height;
            }

            if (_pkgBody != null)
            {
                _pkgBody.Width = spaceWidth;
                _pkgBody.Height = spaceHeight - BOTTOM_HEIGHT;
                _bottomPartTop = _pkgBody.Bottom;
            }
        }

        private void UpdateButtons()
        {
            UpdateButtonStates();
            UpdateButtonLayout();
        }

        private void UpdateButtonStates()
        {
            if (_downloadButton != null)
            {
                _downloadButton.BasicTooltipText = DownloadTooltip;
                _downloadButton.Enabled = (CanDownload || CanUpdate) && !_lockAllButtons;
                if (CanUpdate)
                {
                    _downloadButton.Text = Strings.Update;
                }
                else if (CanDownload)
                {
                    _downloadButton.Text = Strings.Download;
                }
                else
                {
                    _downloadButton.Text = Strings.Download;
                    if (string.IsNullOrWhiteSpace(DownloadTooltip))
                    {
                        _downloadButton.BasicTooltipText = Strings.RelationshipCurrentVersion;
                    }
                }
            }

            if (_infoButton != null)
            {
                bool validUrl = Url.IsValid(PackInfoUrl);
                string tooltip = PackInfoUrl;
                if (!validUrl)
                {
                    tooltip = $"Invalid URL: {PackInfoUrl}";
                }

                _infoButton.Visible = !string.IsNullOrWhiteSpace(PackInfoUrl);
                _infoButton.Enabled = validUrl && !_lockAllButtons;
                _infoButton.BasicTooltipText = tooltip;
            }

            if (_deleteButton != null)
            {
                _deleteButton.BasicTooltipText = DeleteTooltip;

                _deleteButton.Enabled = CanDelete && !_lockAllButtons;
                _deleteButton.Visible = CanDelete && !_lockAllButtons;
            }

            if (_enableButton != null)
            {
                _enableButton.Enabled = CanEnable && !_lockAllButtons;
                _enableButton.Visible = (IsEnabled || CanEnable);

                _enableButton.Text = IsEnabled ? Strings.Disable : Strings.Enable;
            }
        }

        private void UpdateButtonLayout()
        {
            int spaceWidth = DEFAULT_WIDTH;

            if (_parent != null)
            {
                spaceWidth = _parent.ContentRegion.Width;
            }

            if (_downloadButton != null)
            {
                int buttonPaddingTop = (BOTTOM_HEIGHT - _downloadButton.Height) / 2;

                _downloadButton.Location = new Point(spaceWidth - _downloadButton.Width - EDGE_PADDING / 2, _bottomPartTop + buttonPaddingTop);
            }

            if (_infoButton != null)
            {
                int paddingRight = EDGE_PADDING / 2;
                if (_downloadButton != null && _downloadButton.Visible)
                {
                    paddingRight += _downloadButton.Width + EDGE_PADDING / 2;
                }

                int buttonPaddingTop = (BOTTOM_HEIGHT - _infoButton.Height) / 2;
                _infoButton.Location = new Point(spaceWidth - _infoButton.Width - paddingRight, _bottomPartTop + buttonPaddingTop);
            }

            if (_deleteButton != null)
            {
                int paddingRight = EDGE_PADDING / 2;
                if (_downloadButton != null && _downloadButton.Visible)
                {
                    paddingRight += _downloadButton.Width + EDGE_PADDING / 2;
                }
                if (_infoButton != null && _infoButton.Visible)
                {
                    paddingRight += _infoButton.Width + EDGE_PADDING / 2;
                }

                int buttonPaddingTop = (BOTTOM_HEIGHT - _deleteButton.Height) / 2;
                _deleteButton.Location = new Point(spaceWidth - _deleteButton.Width - paddingRight, _bottomPartTop + buttonPaddingTop);
            }

            if (_enableButton != null)
            {
                int paddingRight = EDGE_PADDING / 2;
                if (_downloadButton != null && _downloadButton.Visible)
                {
                    paddingRight += _downloadButton.Width + EDGE_PADDING / 2;
                }
                if (_infoButton != null && _infoButton.Visible)
                {
                    paddingRight += _infoButton.Width + EDGE_PADDING / 2;
                }
                if (_deleteButton != null && _deleteButton.Visible)
                {
                    paddingRight += _deleteButton.Width + EDGE_PADDING / 2;
                }

                int buttonPaddingTop = (BOTTOM_HEIGHT - _enableButton.Height) / 2;
                _enableButton.Location = new Point(spaceWidth - _enableButton.Width - paddingRight, _bottomPartTop + buttonPaddingTop);
            }

            if (_enableButton?.Visible == true)
            {
                _buttonsLeft = _enableButton.Left;
            }
            else if (_deleteButton?.Visible == true)
            {
                _buttonsLeft = _deleteButton.Left;
            }
            else if (_infoButton?.Visible == true)
            {
                _buttonsLeft = _infoButton.Left;
            }
            else if (_downloadButton?.Visible == true)
            {
                _buttonsLeft = _downloadButton.Left;
            }
            else
            {
                _buttonsLeft = spaceWidth - EDGE_PADDING / 2;
            }

            UpdateTagsLayout();
        }

        private void UpdateTagsLayout()
        {
            if (_tagsLabel == null)
            {
                return;
            }

            int spaceWidth = DEFAULT_WIDTH;

            if (_parent != null)
            {
                spaceWidth = _parent.ContentRegion.Width;
            }

            _tagsLabel.Width = spaceWidth - _buttonsLeft - EDGE_PADDING / 2 - _tagsLabel.Left;
        }

        private void UpdateTags()
        {
            if (_tagsLabel == null)
            {
                return;
            }

            _tagsLabel.Text = string.Join(", ", Tags);
        }

        protected override void Unload()
        {
            DownloadClicked = null;
            InfoClicked = null;
            DeleteClicked = null;

            if (_parent != null)
            {
                _parent.Resized -= OnParentResized;
                _parent = null;
            }

            if (_pkgBody != null)
            {
                _pkgBody?.Dispose();
                _pkgBody = null;
            }

            if (_bottomPanel != null)
            {
                _bottomPanel?.Dispose();
                _bottomPanel = null;
            }

            if (_tagsLabel != null)
            {
                _tagsLabel?.Dispose();
                _tagsLabel = null;
            }

            if (_downloadButton != null)
            {
                _downloadButton.Click -= OnDownloadButtonClick;
                _downloadButton?.Dispose();
                _downloadButton = null;
            }

            if (_infoButton != null)
            {
                _infoButton.Click -= OnInfoButtonClick;
                _infoButton?.Dispose();
                _infoButton = null;
            }

            if (_deleteButton != null)
            {
                _deleteButton.Click -= OnDeleteButtonClick;
                _deleteButton?.Dispose();
                _deleteButton = null;
            }

            base.Unload();
        }
    }
}
