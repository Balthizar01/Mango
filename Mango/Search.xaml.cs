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
using System.Windows.Shapes;
using System.Threading;
using MahApps.Metro.Controls;
using Mango.Core.Database;
using Mango.Core.Database.Impl;
using Mango.Core.Model;
using Mango.Core.GUI;

namespace Mango
{
    class Request
    {
        public MangaBox box;
        public Manga manga;
    }
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search
    {
        private static readonly MangaDatabase[] DATABASE = new MangaDatabase[] {
            new MangaReaderDatabase()
        };

        private bool searching;
        public Search()
        {
            InitializeComponent();
            Loader.Visibility = System.Windows.Visibility.Hidden;
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
            Tiles.Children.Add(tile);

            Request request = new Request();
            request.manga = m;
            request.box = box;
            this.request.Add(request);
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
                        Dispatcher.Invoke(new Action(delegate
                        {
                            AddMangaTile(m);
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
                    Dispatcher.Invoke(new Action(delegate
                    {
                        AddMangaTile(m);
                    }));
                }
            }

            if (Tiles.Children.Count == 0)
            {
                UIElement element = FragmentHelper.ExtractUI<NoMangaFound>();
                ScrollView.Content = element;
            }


            new Thread(new ThreadStart(delegate
            {
                foreach (Request request in this.request) {
                    ImageSource img = request.manga.GetCover();
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        request.box.MangaCover = img;
                    }));
                    Thread.Sleep(10);
                }

                Dispatcher.Invoke(new Action(delegate
                {
                    SearchBtn.Content = "Search";
                    Loader.Visibility = System.Windows.Visibility.Hidden;
                    searching = false;
                }));
            })).Start();
        }
    }
}
