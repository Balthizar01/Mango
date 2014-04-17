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
using Mango.Core.Database;
using Mango.Core.Database.Impl;
using System.Threading;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Windows.Controls.Primitives;

namespace Mango
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public class Request
        {
            public Mango.Core.Model.Manga manga;
            public MangaBox box;
        }
        private static readonly MangaDatabase[] DATABASE = new MangaDatabase[] {
            new MangaReaderDatabase(),
            new MangaHereDatabase()
        };

        private bool searching;
        public MainWindow()
        {
            InitializeComponent();

            settingsPane.IsOpenChanged += delegate
            {
                Properties.Settings.Default.Save();
            };
            this.Title = "Mango";
            Loader.Visibility = System.Windows.Visibility.Hidden;
        }

        UniformGrid grid;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AlertUpdates();
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

        private async void AlertUpdates()
        {
            if (!Directory.Exists("mangas"))
                Directory.CreateDirectory("mangas");

            if (!File.Exists("mangas/u1"))
            {
                await this.ShowMessageAsync("What's New?", "* Redesigned Main Menu\n* Added new back and foward buttons in Reader\n* Page up, Page down, Home, and End buttons now function as foward and backwards", MessageDialogStyle.Affirmative);
                File.WriteAllBytes("mangas/u1", new byte[] { 1 });
            }
            else if (!File.Exists("mangas/u2"))
            {

                await this.ShowMessageAsync("What's New?", "* Added MangaHere to search\n* Fixed buttons on reader page\n", MessageDialogStyle.Affirmative);
                File.WriteAllBytes("mangas/u2", new byte[] { 1 });
            }
        }

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
                this.Hide();
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

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!searching)
            {
                searching = true;
                SearchBtn.Content = "Cancel";
                Loader.Visibility = System.Windows.Visibility.Visible;
                SearchForManga();
            }
            else
            {
                SearchBtn.Content = "Search";
                Loader.Visibility = System.Windows.Visibility.Hidden;
                searching = false;
            }
        }

        List<Request> request = new List<Request>();
        private void AddMangaSearchTile(Manga m, ImageSource img)
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
                this.Hide();
            };
            Tiles.Children.Add(tile);

            box.MangaCover = img;
        }

        private async void SearchForManga()
        {
            Tiles.Children.Clear();
            ScrollView.Content = Tiles;

            List<Manga> mangas = new List<Manga>();
            List<Manga> lastResult = null;
            foreach (MangaDatabase db in DATABASE)
            {
                if (!searching)
                    break;
                Task<List<Manga>> result = db.Search(SearchBox.Text);
                if (lastResult != null)
                {
                    foreach (Manga m in lastResult)
                    {
                        ImageSource img = m.GetCover();
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            AddMangaSearchTile(m, img);
                        }));
                    }
                }
                await result;
                lastResult = result.Result;
            }

            if (lastResult != null)
            {
                foreach (Manga m in lastResult)
                {
                    ImageSource img = m.GetCover();
                    await Dispatcher.BeginInvoke(new Action(delegate
                    {
                        AddMangaSearchTile(m, img);
                    }));
                }
            }

            if (Tiles.Children.Count == 0)
            {
                UIElement element = FragmentHelper.ExtractUI<NoMangaFound>();
                ScrollView.Content = element;
            }

            Dispatcher.Invoke(new Action(delegate
            {
                SearchBtn.Content = "Search";
                Loader.Visibility = System.Windows.Visibility.Hidden;
                searching = false;
            }));
        }
    }
}
