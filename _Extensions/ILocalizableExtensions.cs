using AchievementLib.Pack;
using Blish_HUD;
using System.Collections.Generic;

namespace Flyga.AdditionalAchievements
{
    public static class ILocalizableExtensions
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(ILocalizableExtensions));

        // TODO: should not be an extension function here
        public static string GetLocaleString(Gw2Sharp.WebApi.Locale locale)
        {
            return locale.ToString();
        }

        /// <inheritdoc cref="ILocalizable.GetLocalized(string)"/>
        public static string GetLocalized(this ILocalizable localizable, Gw2Sharp.WebApi.Locale locale)
        {
            string localeString = GetLocaleString(locale);

            return localizable.GetLocalized(localeString);
        }

        /// <inheritdoc cref="ILocalizable.GetLocalized(string, string)"/>
        public static string GetLocalized(this ILocalizable localizable, Gw2Sharp.WebApi.Locale locale, Gw2Sharp.WebApi.Locale fallbackLocale)
        {
            string localeString = GetLocaleString(locale);
            string fallbackLocaleString = GetLocaleString(fallbackLocale);

            return localizable.GetLocalized(localeString, fallbackLocaleString);
        }


        /// <inheritdoc cref="ILocalizable.GetLocalized(string, IEnumerable{ILocalizable})"/>
        public static string GetLocalized(this ILocalizable localizable, Gw2Sharp.WebApi.Locale locale, IEnumerable<ILocalizable> references)
        {
            string localeString = GetLocaleString(locale);

            return localizable.GetLocalized(localeString, references);
        }

        /// <inheritdoc cref="ILocalizable.GetLocalized(string, IEnumerable{ILocalizable}, string)"/>
        public static string GetLocalized(this ILocalizable localizable, Gw2Sharp.WebApi.Locale locale, IEnumerable<ILocalizable> references, Gw2Sharp.WebApi.Locale fallbackLocale)
        {
            string localeString = GetLocaleString(locale);
            string fallbackLocaleString = GetLocaleString(fallbackLocale);

            return localizable.GetLocalized(localeString, references, fallbackLocaleString);
        }

        public static string GetLocalizedForUserLocale(this ILocalizable localizable, Gw2Sharp.WebApi.Locale fallbackLocale = Gw2Sharp.WebApi.Locale.English)
        {
            string userLocaleString = GetLocaleString(GameService.Overlay.UserLocale.Value);
            string fallbackLocaleString = GetLocaleString(fallbackLocale);

            return localizable.GetLocalized(userLocaleString, fallbackLocaleString);
        }

        public static string GetLocalizedForUserLocale(this ILocalizable localizable, IEnumerable<ILocalizable> references, Gw2Sharp.WebApi.Locale fallbackLocale = Gw2Sharp.WebApi.Locale.English)
        {
            string userLocaleString = GetLocaleString(GameService.Overlay.UserLocale.Value);
            string fallbackLocaleString = GetLocaleString(fallbackLocale);

            return localizable.GetLocalized(userLocaleString, references, fallbackLocaleString);
        }
    }
}
