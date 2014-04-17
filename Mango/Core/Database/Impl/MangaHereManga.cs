using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mango.Core.Model;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Net;
using System.IO;


namespace Mango.Core.Database.Impl
{
    public class MangaHereManga : Manga
    {
        private int page = 1;
        private int chapter = 1;
        private int volume = 0;
        internal bool usesVolumes = false;
        internal MangaHereDatabase db;
        internal string ImageURL;
        internal string PageURL;

        private bool DoesExist(int volume, int chapter, int page)
        {
            string url;
            if (volume > 0)
            {
                string vstr = "v" + (volume < 10 ? "0" + volume : "" + volume);
                string cstr = "c" + (chapter < 100 && chapter >= 10 ? "0" + chapter : (chapter < 10 ? "00" + chapter : "" + chapter));

                if (page != 1) url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + vstr + "/" + cstr + "/" + page + ".html";
                else url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + vstr + "/" + cstr + "/";
            }
            else
            {
                string cstr = "c" + (chapter < 100 && chapter >= 10 ? "0" + chapter : (chapter < 10 ? "00" + chapter : "" + chapter));

                if (page != 1) url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + cstr + "/" + page + ".html";
                else url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + cstr + "/";
            }

            string result;
            try
            {
                using (WebClient client = new WebClient())
                {
                    result = client.DownloadString(url);
                }
            }
            catch
            {
                return false;
            }

            if (String.IsNullOrWhiteSpace(result))
                return false;
            return !result.Contains("is not available yet.") && !result.Contains("have requested can’t be found.");
        }

        private int NextType(int volume, int chapter, int page)
        {
            if (DoesExist(volume, chapter, page + 1))
                return 1;
            else if (DoesExist(volume, chapter + 1, 1))
                return 2;
            else if (UsesVolumes && DoesExist(volume + 1, chapter + 1, 1))
                return 3;
            return 0;
        }

        private int PreviousType(int volume, int chapter, int page)
        {
            if (DoesExist(volume, chapter, page - 1))
                return 1;
            else if (DoesExist(volume, chapter - 1, 1))
                return 2;
            else if (DoesExist(volume - 1, chapter - 1, 1))
                return 3;
            return 0;
        }

        private int NextType()
        {
            if (DoesExist(volume, chapter, page + 1))
                return 1;
            else if (DoesExist(volume, chapter + 1, 1))
                return 2;
            else if (UsesVolumes && DoesExist(volume + 1, chapter + 1, 1))
                return 3;
            return 0;
        }

        private int PreviousType()
        {
            if (DoesExist(volume, chapter, page - 1))
                return 1;
            else if (DoesExist(volume, chapter - 1, 1))
                return 2;
            else if (DoesExist(volume - 1, chapter - 1, 1))
                return 3;
            return 0;
        }

        public override bool HasNext()
        {
            return NextType() != 0;
        }

        public override bool HasPrevious()
        {
            return PreviousType() != 0;
        }

        public override async Task<bool> Next()
        {
            Task<int> result = Task.Run<int>(new Func<int>(NextType));
            await result;

            if (result.Result == 0)
                return false;
            else
            {
                img = null;
                int r = result.Result;
                if (r == 1)
                    page++;
                else if (r == 2)
                {
                    page = 1;
                    chapter++;
                }
                else if (r == 3)
                {
                    page = 1;
                    chapter++;
                    volume++;
                }
            }
            return true;
        }

        public override async Task<bool> Previous()
        {
            Task<int> result = Task.Run<int>(new Func<int>(PreviousType));
            await result;

            if (result.Result == 0)
                return false;
            else
            {
                img = null;
                int r = result.Result;
                if (r == 1)
                    page--;
                else if (r == 2)
                {
                    page = 1;
                    chapter--;
                }
                else if (r == 3)
                {
                    page = 1;
                    chapter--;
                    volume--;
                }
            }
            return true;
        }

        private string ImageUrlFor(int volume, int chapter, int page)
        {
            string url;
            if (volume > 0)
            {
                string vstr = "v" + (volume < 10 ? "0" + volume : "" + volume);
                string cstr = "c" + (chapter < 100 && chapter >= 10 ? "0" + chapter : (chapter < 10 ? "00" + chapter : "" + chapter));

                if (page != 1) url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + vstr + "/" + cstr + "/" + page + ".html";
                else url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + vstr + "/" + cstr + "/";
            }
            else
            {
                string cstr = "c" + (chapter < 100 && chapter >= 10 ? "0" + chapter : (chapter < 10 ? "00" + chapter : "" + chapter));

                if (page != 1) url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + cstr + "/" + page + ".html";
                else url = "http://www.mangahere.com/manga/" + Title.ToLower().Replace(' ', '_') + "/" + cstr + "/";
            }

            string result;
            try
            {
                using (WebClient client = new WebClient())
                {
                    result = client.DownloadString(url);
                }
            }
            catch
            {
                return null;
            }

            if (String.IsNullOrWhiteSpace(result))
                return null;

            string imgurl = "";
            string[] lines = result.Split('\n');
            bool found = false;
            foreach (string l in lines)
            {
                if (!found)
                {
                    if (l.Contains("<section class=\"read_img\" id=\"viewer\">"))
                    {
                        found = true;
                    }
                }
                else
                {
                    if (l.Contains("<img src="))
                    {
                        bool start = false;
                        int indexof = result.IndexOf("<img src=");
                        for (int i = indexof; i < result.Length; i++)
                        {
                            if (!start && result[i] == '"')
                            {
                                start = true;
                                continue;
                            }
                            else if (start && result[i] == '"')
                            {
                                break;
                            }
                            else if (start)
                            {
                                imgurl += result[i];
                            }
                        }

                        return imgurl;
                    }
                }
            }

            return imgurl;
        }

        private BitmapImage img;
        public override void PrepareDisplay()
        {
            try
            {
                PrepareDisplay(false);
            }
            catch
            {
                //Try once more but force web
                PrepareDisplay(true);
            }

            if (img != null && !img.IsFrozen)
                img.Freeze();
        }

        private void PrepareDisplay(bool forceWeb)
        {
            if (img != null)
                return;
            byte[] byteData = new byte[0];
            if (!IsDownloaded)
            {
                string url = ImageUrlFor(CurrentVolume, CurrentChapter, CurrentPage);
                if (url == null)
                {
                    System.Windows.MessageBox.Show("Error finding image!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        if (!forceWeb && (App.CachePages || (IsDownloaded && !File.Exists("mangas/" + MakeValidFileName(Title) + "/" + CurrentChapter + "-" + CurrentPage))))
                        {
                            if (!Directory.Exists("mangas"))
                                Directory.CreateDirectory("mangas");
                            if (!Directory.Exists("mangas/" + MakeValidFileName(Title)))
                                Directory.CreateDirectory("mangas/" + MakeValidFileName(Title));

                            client.DownloadFile(url, "mangas/" + MakeValidFileName(Title) + "/" + CurrentChapter + "-" + CurrentPage);
                        }
                        else
                            byteData = client.DownloadData(url);
                    }
                }
                catch
                {
                    System.Windows.MessageBox.Show("Error downloading found image!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
            }

            img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            if (!IsDownloaded)
                img.StreamSource = new MemoryStream(byteData);
            else
                img.UriSource = new Uri("mangas/" + MakeValidFileName(Title) + "/" + CurrentChapter + "-" + CurrentPage, UriKind.Relative);
            img.EndInit();
            img.Freeze();
        }

        public override void Display(System.Windows.Controls.Grid displayGrid)
        {
            if (img == null)
                return;

            Image image = new Image();
            image.Stretch = Stretch.Fill;
            image.Width = img.Width;
            image.Height = img.Height;
            image.Source = img;

            displayGrid.Children.Add(image);
        }

        public override ImageSource GetCover()
        {
            try
            {
                BitmapImage bImage = new BitmapImage();

                bImage.BeginInit();
                if (App.CacheImages)
                {
                    bImage.UriSource = downloadImage();
                    if (bImage.UriSource == null)
                    {
                        return null;
                    }
                }
                else
                {
                    byte[] data;
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            data = client.DownloadData(ImageURL);
                        }

                        bImage.StreamSource = new MemoryStream(data);
                    }
                    catch (Exception e)
                    {
                        if (e.ToString().Contains("(404) Not Found")) return null;
                        System.Windows.MessageBox.Show("Title: " + Title + "\n" + "URL: " + ImageURL + "\n" + e.ToString());
                    }
                }
                bImage.CacheOption = BitmapCacheOption.OnLoad;
                bImage.DecodePixelWidth = 255;
                bImage.EndInit();
                bImage.Freeze();
                return bImage;
            }
            catch
            {
                return null;
            }
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private Uri downloadImage()
        {
            string title = MakeValidFileName(Title);
            if (!Directory.Exists("imgcache"))
                Directory.CreateDirectory("imgcache");
            if (File.Exists("imgcache/manga_here_" + title + ".jpg"))
                return new Uri("imgcache/manga_here_" + title + ".jpg", UriKind.Relative);

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(ImageURL, "imgcache/manga_here_" + title + ".jpg");
                    client.Dispose();
                }
                catch (FileNotFoundException e)
                {
                    return null;
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("(404) Not Found")) return null;
                    System.Windows.MessageBox.Show("Title: " + title + "\n" + "URL: " + ImageURL + "\n" + e.ToString());
                }
            }

            return new Uri("imgcache/manga_here_" + title + ".jpg", UriKind.Relative);
        }

        public override bool IsDownloaded
        {
            get { return !(dPage == CurrentPage && dChapter == CurrentChapter && dVolume == CurrentVolume) && File.Exists("mangas/" + MakeValidFileName(Title) + "/" + CurrentVolume + "-" + CurrentChapter + "-" + CurrentPage) && new FileInfo("mangas/" + MakeValidFileName(Title) + "/" + CurrentChapter + "-" + CurrentPage).Length > 0; }
        }

        public override bool IsDownloading
        {
            get { return download; }
        }

        private bool download = false;
        private bool cancel = false;
        public override void CancelDownload(System.Threading.Thread downloadThread)
        {
            cancel = true;
            if (downloadThread != null)
            {
                downloadThread.Abort();
                downloadThread.Join(3000);
            }
        }

        private int dVolume, dPage, dChapter;
        public override void Download()
        {
            if (download)
                return;
            download = true;
            cancel = false;
            if (!Directory.Exists("mangas"))
                Directory.CreateDirectory("mangas");
            string folder = MakeValidFileName(Title);
            Directory.CreateDirectory("mangas/" + folder);

            dPage = 0;
            dChapter = 1;
            dVolume = 0;
            if (UsesVolumes)
                dVolume = 1;
            int nType = 0;

            while ((nType = NextType(dVolume, dChapter, dPage)) != 0)
            {
                if (cancel)
                    break;
                if (nType == 1)
                    dPage++;
                else if (nType == 2)
                {
                    dChapter++;
                    dPage = 1;
                }
                else if (nType == 3)
                {
                    dVolume++;
                    dChapter++;
                    dPage = 1;
                }

                if (File.Exists("mangas/" + folder + "/" + dVolume + "-" + dChapter + "-" + dPage))
                    continue;

                string url = ImageUrlFor(dVolume, dChapter, dPage);
                if (url == null)
                {
                    //TODO break?
                    continue;
                    //System.Windows.MessageBox.Show("Error finding image!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(url, "mangas/" + folder + "/" + dVolume + "-" + dChapter + "-" + dPage);
                    }
                }
                catch
                {
                    //TODO break?
                    continue;
                }
            }

            if (!cancel)
                File.WriteAllBytes("mangas/" + MakeValidFileName(Title) + "/completed.dat", new byte[] { 1 });

            cancel = false;
            download = false;
        }

        public override bool IsDownloadComplete
        {
            get { return File.Exists("mangas/" + MakeValidFileName(Title) + "/completed.dat"); }
        }

        public override MangaDatabase DatabaseParent
        {
            get { return db; }
        }

        public override int CurrentPage
        {
            get
            {
                return page;
            }
            internal set
            {
                this.page = value;
            }
        }

        public override int CurrentChapter
        {
            get
            {
                return chapter;
            }
            internal set
            {
                this.chapter = value;
            }
        }

        public override int CurrentVolume
        {
            get
            {
                return volume;
            }
            internal set
            {
                this.volume = value;
            }
        }

        public override bool UsesVolumes
        {
            get
            {
                return usesVolumes;
            }
            internal set
            {
                this.usesVolumes = value;
            }
        }
    }
}
