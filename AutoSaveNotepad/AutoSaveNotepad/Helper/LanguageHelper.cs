using AutoSaveNotepad.Localization;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AutoSaveNotepad.Helper
{
    public static class LanguageHelper
    {
        public static void SetApplicationLanguage(string selectedLanguage)
        {
            // Get current UI culture
            CultureInfo language = Thread.CurrentThread.CurrentUICulture;

            // Get culture info of passed two letter language code
            if (string.IsNullOrEmpty(selectedLanguage) == false)
            {
                language = CultureInfo.GetCultures(CultureTypes.NeutralCultures).First(x => x.TwoLetterISOLanguageName.ToUpper().Equals(selectedLanguage));
            }

            // Set UI culture and localization
            Thread.CurrentThread.CurrentUICulture = language;
            AppResources.Culture = language;

            // Save selected language in app settings
            Properties.Settings.Default.Application_Language = selectedLanguage;
            Properties.Settings.Default.Save();
        }
    }
}
