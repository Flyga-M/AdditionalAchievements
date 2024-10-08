﻿using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Repo;
using Flyga.AdditionalAchievements.UI.Views;
using System.Linq;

namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class RepoPresenter : Presenter<RepoView, AchievementPackRepo>
    {
        public RepoPresenter(RepoView view, AchievementPackRepo model) : base(view, model)
        {
        }

        protected override void UpdateView()
        {
            View.SetContent(Model.AchievementPackages.Select(pkg => new PkgView(pkg)));
        }
    }
}
