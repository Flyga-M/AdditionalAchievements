using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Flyga.AdditionalAchievements.Solve.Handler;
using Flyga.AdditionalAchievements.UI.Views;
using System.Linq;


namespace Flyga.AdditionalAchievements.UI.Presenters
{
    public class AchievementWindowPresenter : Presenter<AchievementWindowView, AchievementHandler>
    {
        private static readonly Logger Logger = Logger.GetLogger<AchievementWindowPresenter>();

        public AchievementWindowPresenter(AchievementWindowView view, AchievementHandler model) : base(view, model)
        { /** NOOP **/ }

        protected override void UpdateView()
        {
            AchievementMenuView menuView = new AchievementMenuView(Model);
            View.MenuView = menuView;

            menuView.SearchTextChanged += OnSearchTextChanged;
        }

        string _searchText = string.Empty;

        private void OnSearchTextChanged(object sender, string text)
        {
            _searchText = text;
            View.OnSubViewClearSelected(sender, () => new AchievementCollectionView(Model.CurrentCategories.SelectMany(category => category.AchievementCollections), FilterSearch));
        }

        private bool FilterSearch(IAchievement achievement)
        {
            // TODO: make fallback locale an option
            return achievement.Name.GetLocalizedForUserLocale().ToLower().Contains(_searchText.ToLower());
        }

        protected override void Unload()
        {
            View.MenuView.SearchTextChanged -= OnSearchTextChanged;
        }
    }
}
