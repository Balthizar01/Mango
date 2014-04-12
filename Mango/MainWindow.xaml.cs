using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mango.Core.GUI;
using MahApps.Metro.Controls;
using Mango.Core.Model;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace Mango
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            settingsPane.IsOpenChanged += delegate
            {
                Properties.Settings.Default.Save();
            };
            this.Title = "Mango";
        }

        UniformGrid grid;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MangaList.List.Count == 0)
            {
                Content.Children.Add(FragmentHelper.ExtractUI<NoManga>());
            }
            else
            {
                grid = new UniformGrid();
                Content.Children.Add(grid);
                foreach (Manga m in MangaList.List)
                {
                    AddMangaTile(m);
                }
                new Thread(new ThreadStart(delegate
                {
                    foreach (Request request in this.request)
                    {
                        ImageSource img = request.manga.GetCover();
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            request.box.MangaCover = img;
                        }));
                        Thread.Sleep(10);
                    }
                })).Start();
                
            }
        }

        List<Request> request = new List<Request>();
        private void AddMangaTile(Manga m)
        {
            MangaBox box = new MangaBox();
            box.MangaTitle = m.Title;
            box.DatabaseText = (m.DatabaseParent == null ? "Local" : m.DatabaseParent.Name);

            Grid grid = FragmentHelper.ExtractUI<Grid>(box);
            Tile tile = new Tile();
            tile.Width = 225;
            tile.Height = 180;
            tile.Content = grid;
            tile.Click += delegate
            {
                Reader read = new Reader(m);
                read.Show();
                this.Close();
            };
            this.grid.Children.Add(tile);

            Request request = new Request();
            request.manga = m;
            request.box = box;
            this.request.Add(request);
        }

        private async void TestDownload()
        {
            MessageBox.Show("Started test download");
            List<Manga> search = await new Mango.Core.Database.Impl.MangaReaderDatabase().Search("madoka");
            search[0].Download();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Search search = new Search();
            search.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            cPages.IsChecked = App.CachePages;
            cManga.IsChecked = App.CacheImages;
            settingsPane.IsOpen = !settingsPane.IsOpen;
        }

        private void cManga_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.cache_images = (bool)cManga.IsChecked;
        }

        private void cPages_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.cache_pages = (bool)cPages.IsChecked;
        }
    }
}
