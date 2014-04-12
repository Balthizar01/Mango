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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MangaList.Load();
        }
    }
}
