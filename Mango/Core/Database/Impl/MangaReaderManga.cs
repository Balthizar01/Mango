using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mango.Core.Model;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Net;
using System.Windows.Controls;

namespace Mango.Core.Database.Impl
{
    public class MangaReaderManga : Manga
    {
        internal MangaDatabase db;
        private int Page = 1;
        private int Chapter = 1;
        public override MangaDatabase DatabaseParent
        {
            get { return db; }
            
        }
        public string PageURL
        {
            get;
            set;
        }

        public string ImageURL
        {
            get;
            set;
        }

        public override bool HasNext()
        {
            string url = ImageUrlFor(CurrentChapter, CurrentPage + 1); //Try next page
            if (url == null) //No image url found
            {
                ImageUrlFor(CurrentChapter + 1, 1); //Try next chapter
                if (url == null) //No image url found
                    return false; //No previous

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadData(url); //Try download
                    }
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("(404) Not Found")) //404?
                        return false; //No previous
                }
                return true; //Previous exists
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadData(url); //Try downloading
                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("(404) Not Found")) //404?
                {
                    ImageUrlFor(CurrentChapter + 1, 1); //Try last chapter
                    if (url == null) //No image url found
                        return false; //No previous

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadData(url); //Try downloading
                        }
                    }
                    catch (Exception ew)
                    {
                        if (ew.ToString().Contains("(404) Not Found")) //404?
                            return false; //No previous
                    }
                    return true; //Previous exists
                }
            }
            return true; //Previous exists
        }

        public override async Task<bool> Next()
        {
            Task<int> result = Task.Run<int>(new Func<int>(NextType));
            await result;

            int type = result.Result;
            if (type == 0)
                return false;
            else
            {
                img = null; //Reset image
                if (type == 1)
                    Page++;
                else
                {
                    Chapter++;
                    Page = 1;
                }
            }
            return true;
        }

        public override async Task<bool> Previous()
        {
            Task<int> result = Task.Run<int>(new Func<int>(PreviousType));
            await result;

            int type = result.Result;
            if (type == 0)
                return false;
            else
            {
                img = null; //Reset image
                if (type == 1)
                    Page--;
                else
                {
                    Chapter--;
                    Page = 1;
                }
            }
            return true;
        }

        public override void Display(System.Windows.Controls.Grid displayGrid)
        {
            if (img == null)
                return;

            Image image = new Image();
            image.Stretch = Stretch.Fill;
            image.Source = img;

            displayGrid.Children.Add(image);
        }

        public override int CurrentPage
        {
            get { return Page; }
            internal set { this.Page = value; }
        }

        public override int CurrentChapter
        {
            get { return Chapter; }
            internal set { this.Chapter = value; }
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
            if (File.Exists("imgcache/manga_reader_" + title + ".jpg"))
                return new Uri("imgcache/manga_reader_" + title + ".jpg", UriKind.Relative);

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(ImageURL, "imgcache/manga_reader_" + title + ".jpg");
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

            return new Uri("imgcache/manga_reader_" + title + ".jpg", UriKind.Relative);
        }

        private int NextType()
        {
            string folder = "mangas/" + MakeValidFileName(Title);
            //First check local
            if (Directory.Exists(folder))
            {
                if (!File.Exists(folder + "/" + CurrentChapter + "-" + (CurrentPage + 1)))
                {
                    if (Directory.GetFiles(folder, (CurrentChapter + 1) + "-*", SearchOption.TopDirectoryOnly).Length > 0)
                        return 2;
                }
                else return 1;
            }
            //Now check online
            string url = ImageUrlFor(CurrentChapter, CurrentPage + 1); //Try last page
            if (url == null) //No image url found
            {
                url = ImageUrlFor(CurrentChapter + 1, 1); //Try last chapter
                if (url == null) //No image url found
                    return 0; //No previous

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadData(url); //Try download
                    }
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("(404) Not Found")) //404?
                        return 0; //No previous
                }
                return 2; //Previous exists
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadData(url); //Try downloading
                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("(404) Not Found")) //404?
                {
                    ImageUrlFor(CurrentChapter + 1, 1); //Try last chapter
                    if (url == null) //No image url found
                        return 0; //No previous

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadData(url); //Try downloading
                        }
                    }
                    catch (Exception ew)
                    {
                        if (ew.ToString().Contains("(404) Not Found")) //404?
                            return 0; //No previous
                    }
                    return 2; //Previous exists
                }
            }
            return 1; //Previous exists
        }

        private int PreviousType()
        {
            string folder = "mangas/" + MakeValidFileName(Title);
            //First check local
            if (Directory.Exists(folder))
            {
                if (!File.Exists(folder + "/" + CurrentChapter + "-" + (CurrentPage - 1)))
                {
                    if (Directory.GetFiles(folder, (CurrentChapter - 1) + "-*", SearchOption.TopDirectoryOnly).Length > 0)
                        return 2;
                }
                else return 1;
            }
            //Now check online
            string url = ImageUrlFor(CurrentChapter, CurrentPage - 1); //Try last page
            if (url == null) //No image url found
            {
                url = ImageUrlFor(CurrentChapter - 1, 1); //Try last chapter
                if (url == null) //No image url found
                    return 0; //No previous

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadData(url); //Try download
                    }
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("(404) Not Found")) //404?
                        return 0; //No previous
                }
                return 2; //Previous exists
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadData(url); //Try downloading
                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("(404) Not Found")) //404?
                {
                    ImageUrlFor(CurrentChapter - 1, 1); //Try last chapter
                    if (url == null) //No image url found
                        return 0; //No previous

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadData(url); //Try downloading
                        }
                    }
                    catch (Exception ew)
                    {
                        if (ew.ToString().Contains("(404) Not Found")) //404?
                            return 0; //No previous
                    }
                    return 2; //Previous exists
                }
            }
            return 1; //Previous exists
        }

        public override bool HasPrevious()
        {
            return PreviousType() != 0;
        }

        private string ImageUrlFor(int chapter, int page)
        {
            string url = "";
            try
            {
                string data;
                using (WebClient client = new WebClient())
                {
                    string temp = "http://www.mangareader.net/" + Title.ToLower().Replace(":", "").Replace("&", "").Replace(' ', '-') + "/" + chapter + "/" + page;
                    data = client.DownloadString(temp);
                }
                string[] lines = data.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Contains("<img id="))
                    {
                        bool start = false;
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (!start)
                            {
                                if (line[i] == 's' && line[i + 1] == 'r' && line[i + 2] == 'c' && line[i + 3] == '=' && line[i + 4] == '"')
                                {
                                    start = true;
                                    i += 4;
                                    continue;
                                }
                            }
                            else
                            {
                                if (line[i] == '"')
                                    break;
                                else
                                    url += line[i];
                            }
                        }
                        break;
                    }
                }
            }
            catch
            {
                return null;
            }

            return url;
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
                return; //We are already prepared

            byte[] byteData = new byte[0];
            if (!IsDownloaded)
            {
                string url = ImageUrlFor(CurrentChapter, CurrentPage);
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

        private int NextType(int chapter, int page)
        {
            string folder = "mangas/" + MakeValidFileName(Title);
            //First check local
            if (Directory.Exists(folder))
            {
                if (!File.Exists(folder + "/" + chapter + "-" + (page + 1)))
                {
                    if (Directory.GetFiles(folder, (chapter + 1) + "-*", SearchOption.TopDirectoryOnly).Length > 0)
                        return 2;
                }
                else
                    return 1;
            }
            //Now check online
            string url = ImageUrlFor(chapter, page + 1); //Try last page
            if (url == null) //No image url found
            {
                url = ImageUrlFor(chapter + 1, 1); //Try last chapter
                if (url == null) //No image url found
                    return 0; //No previous

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadData(url); //Try download
                    }
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("(404) Not Found")) //404?
                        return 0; //No previous
                }
                return 2; //Previous exists
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadData(url); //Try downloading
                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("(404) Not Found")) //404?
                {
                    ImageUrlFor(chapter + 1, 1); //Try last chapter
                    if (url == null) //No image url found
                        return 0; //No previous

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadData(url); //Try downloading
                        }
                    }
                    catch (Exception ew)
                    {
                        if (ew.ToString().Contains("(404) Not Found")) //404?
                            return 0; //No previous
                    }
                    return 2; //Previous exists
                }
            }
            return 1; //Previous exists
        }

        public override bool IsDownloaded
        {
            get { return !(dPage == CurrentPage && dChapter == CurrentChapter) && File.Exists("mangas/" + MakeValidFileName(Title) + "/" + CurrentChapter + "-" + CurrentPage) && new FileInfo("mangas/" + MakeValidFileName(Title) + "/" + CurrentChapter + "-" + CurrentPage).Length > 0; }
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

        private int dPage, dChapter;
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
            int nType = 0;

            while ((nType = NextType(dChapter, dPage)) != 0)
            {
                if (cancel)
                    break;
                if (nType == 1)
                    dPage++;
                else
                {
                    dChapter++;
                    dPage = 1;
                }

                if (File.Exists("mangas/" + folder + "/" + dChapter + "-" + dPage))
                    continue;

                string url = ImageUrlFor(dChapter, dPage);
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
                        client.DownloadFile(url, "mangas/" + folder + "/" + dChapter + "-" + dPage);
                    }
                }
                catch
                {
                    //TODO break?
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
    }
}

