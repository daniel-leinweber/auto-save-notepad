using AutoSaveNotepad.Helper;
using AutoSaveNotepad.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AutoSaveNotepad
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args;

        public App()
        {
            InitializeApplicationLanguage();
        }

        private void InitializeApplicationLanguage()
        {
            var applicationLanguage = Settings.Default.Application_Language;
            LanguageHelper.SetApplicationLanguage(applicationLanguage);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                Args = e.Args;
            }
        }
    }
}
