﻿using System;
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
using Mango.Core.Model;
using System.Threading;
using MahApps.Metro.Controls.Dialogs;

namespace Mango
{
    /// <summary>
    /// Interaction logic for Reader.xaml
    /// </summary>
    public partial class Reader
    {
        private Manga manga;
        public Reader(Manga manga)
        {
            this.manga = manga;
            InitializeComponent();
            PageContent.Visibility = System.Windows.Visibility.Hidden;
            this.Closed += delegate
            {
                MangaList.Save();
                App.Window.Show();
            };
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (manga.IsDownloaded || manga.IsDownloadComplete)
                this.downloadSetting.IsChecked = true;

            this.KeyDown += Reader_KeyDown;
            this.KeyUp += Reader_KeyUp;
            PageContent.KeyUp += PageContent_KeyUp;
            PageContent.KeyDown += PageContent_KeyDown;

            this.WindowState = System.Windows.WindowState.Maximized;
            //NextBtn.IsEnabled = false;
            //PreviousBtn.IsEnabled = false;
            //NextBtn.Content = "Loading..";
            //PreviousBtn.Content = "Loading..";
            new Thread(new ThreadStart(Setup)).Start();
        }

        private bool pressed2;
        void PageContent_KeyDown(object sender, KeyEventArgs e)
        {
            pressed2 = e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Home || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key == Key.End;
        }

        void PageContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (!pressed) return;
            if (e.Key == Key.Left || e.Key == Key.End)
            {
                Previous();
            }
            else if (e.Key == Key.Right || e.Key == Key.Home)
            {
                Next();
            }
            else if (e.Key == Key.PageUp)
            {
                Scroller.ScrollToVerticalOffset(0);
            }
            else if (e.Key == Key.PageDown)
            {
                Scroller.ScrollToBottom();
            }
        }

        bool pressed = false;
        void Reader_KeyUp(object sender, KeyEventArgs e)
        {
            if (!pressed) return;
            if (e.Key == Key.Left || e.Key == Key.End)
            {
                Previous();
            }
            else if (e.Key == Key.Right || e.Key == Key.Home)
            {
                Next();
            }
            else if (e.Key == Key.PageUp)
            {
                Scroller.ScrollToVerticalOffset(0);
            }
            else if (e.Key == Key.PageDown)
            {
                Scroller.ScrollToBottom();
            }
        }

        void Reader_KeyDown(object sender, KeyEventArgs e)
        {
            pressed = e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Home || e.Key == Key.End || e.Key == Key.PageUp || e.Key == Key.PageDown;
        }

        private string GetTitle()
        {
            string title;
            if (manga.Title.Length > 25)
                title = manga.Title.Substring(0, 25 - 3) + "...";
            else
                title = manga.Title;
            title += " - Chapter: " + manga.CurrentChapter + " Page: " + manga.CurrentPage;

            return title;
        }

        private void Setup()
        {
            manga.PrepareDisplay();
            Dispatcher.Invoke(new Action(delegate
            {
                PageContent.Children.Clear(); //In case it didn't get cleared..?
                manga.Display(PageContent);
                Scroller.ScrollToVerticalOffset(0);
                Scroller.ScrollToHorizontalOffset(0);
                this.Title = GetTitle();
                PageContent.Visibility = System.Windows.Visibility.Visible;
                Loader.Visibility = System.Windows.Visibility.Hidden;
            }));

            /*try
            {
                if (manga.HasNext())
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (!this.IsVisible)
                            return;
                        NextBtn.Content = "Next";
                        NextBtn.IsEnabled = true;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (!this.IsVisible)
                            return;
                        NextBtn.Content = "No Next";
                    }));
                }

                if (manga.HasPrevious())
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (!this.IsVisible)
                            return;
                        PreviousBtn.Content = "Previous";
                        PreviousBtn.IsEnabled = true;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        if (!this.IsVisible)
                            return;
                        PreviousBtn.Content = "No Previous";
                    }));
                }
            }
            catch {  }*/
        }

        private async void Next()
        {
            Task<bool> result = manga.Next();
            Loader.Visibility = System.Windows.Visibility.Visible;
            await result;
            if (result.Result)
            {
                PageContent.Visibility = System.Windows.Visibility.Hidden;
                //NextBtn.IsEnabled = false;
                //PreviousBtn.IsEnabled = false;
                //NextBtn.Content = "Loading..";
                //PreviousBtn.Content = "Loading..";
                new Thread(new ThreadStart(Setup)).Start();
            }
            else
            {
                Loader.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private async void Previous()
        {
            Task<bool> result = manga.Previous();
            Loader.Visibility = System.Windows.Visibility.Visible;
            await result;
            if (result.Result)
            {
                PageContent.Visibility = System.Windows.Visibility.Hidden;
                //NextBtn.IsEnabled = false;
                //PreviousBtn.IsEnabled = false;
                //NextBtn.Content = "Loading..";
                //PreviousBtn.Content = "Loading..";
                new Thread(new ThreadStart(Setup)).Start();
            }
            else
            {
                Loader.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void PreviousBtn_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            App.Window.Show();
            this.Close();
        }

        private void downloadSetting_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)downloadSetting.IsChecked)
            {
                DownloadManga();
            }
            else
            {
                manga.CancelDownload(downloadThread);
                Manga toremove = null;
                foreach (Manga m in MangaList.List)
                {
                    if (m.Title == manga.Title)
                    {
                        toremove = m;
                        break;
                    }
                }

                if (toremove != null)
                {
                    MangaList.List.Remove(toremove);
                    MangaList.Save();
                }
            }
        }

        Thread downloadThread;
        private async void DownloadManga()
        {
            MangaList.List.Add(manga);
            MangaList.Save();
            await this.ShowMessageAsync("Mango", manga.Title + " will download in the background while you read. If you close Mango, the download will pause and resume the next time you start reading.", MessageDialogStyle.Affirmative);
            downloadThread = new Thread(new ThreadStart(delegate
            {
                manga.Download();
            }));
            downloadThread.Start();
        }
    }
}
