using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Mango.Core.Model;

namespace Mango
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow Window;
        public static bool CacheImages
        {
            get
            {
                return Mango.Properties.Settings.Default.cache_images;
            }
        }

        public static bool CachePages
        {
            get
            {
                return Mango.Properties.Settings.Default.cache_pages;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            foreach (Manga m in MangaList.List)
            {
                if (m.IsDownloading)
                {
                    m.CancelDownload();
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!IsNet45OrNewer())
            {
                Mango.Core.WrongNet window = new Core.WrongNet();
                window.Show();
            }
            else
            {
                MangaList.Load();
                Window = new Mango.MainWindow();
                Window.Show();
            }
        }



        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MangaList.Load();
        }

        public static bool IsNet45OrNewer()
        {
            // Class "ReflectionContext" exists from .NET 4.5 onwards.
            return Type.GetType("System.Reflection.ReflectionContext", false) != null;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

        }
    }
}
