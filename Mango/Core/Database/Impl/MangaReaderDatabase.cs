using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Mango.Core.Database.Impl
{
    public class MangaReaderDatabase : MangaDatabase
    {
        public string Name
        {
            get
            {
                return "MangaReader";
            }
        }

        public string SiteURL
        {
            get
            {
                return "http://http://www.mangareader.net/";
            }
        }

        readonly List<char> numbers = new List<char>(new char[] {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
        });
        public async Task<List<Model.Manga>> Search(string keyword)
        {
            string results;
            using (WebClient client = new WebClient())
            {
                try
                {
                    results = await client.DownloadStringTaskAsync("http://www.mangareader.net/search/?w=" + keyword);
                }
                catch
                {
                    return new List<Model.Manga>();
                }
            }

            List<Model.Manga> mangas = new List<Model.Manga>();
            string[] lines = results.Split('\n');
            bool resultFound = false;
            bool infoFound = false;
            int chapterCount = 0;
            string title = "";
            string url = "";
            string imgUrl = "";
            foreach (string line in lines)
            {
                if (!resultFound && line.Contains("<div class=\"mangaresultitem\""))
                {
                    resultFound = true;
                }
                else if (resultFound)
                {
                    if (line.Contains("<div class=\"clear\""))
                    {
                        infoFound = false;
                        resultFound = false;
                        continue;
                    }
                    if (!infoFound)
                    {
                        if (line.Contains("<div class=\"result_info c4\""))
                        {
                            infoFound = true;
                        }
                    }
                    else
                    {
                        if (line.Contains("<a href=\""))
                        {
                            if (line.Contains("<h3>"))
                            {
                                title = line.Split('>')[2].Replace("</a", "");
                                url = line.Split('>')[1].Replace("<a href=\"", "").Replace("\"", "");
                                string temp;
                                using (WebClient client = new WebClient())
                                {
                                    try
                                    {
                                        temp = await client.DownloadStringTaskAsync("http://www.mangareader.net" + url);

                                        string[] ll = temp.Split('\n');
                                        foreach (string l in ll)
                                        {
                                            if (l.Contains("<div id=\"mangaimg\">"))
                                            {
                                                imgUrl = l.Replace("<div id=\"mangaimg\">", "").Replace("<img src=\"", "").Replace("\" alt=\"" + title + " Manga\" /></div>", "");
                                                break;
                                            }
                                        }
                                    }
                                    catch {}
                                }
                            }
                            else
                            {
                                title = line.Split('>')[1].Replace("</a", "");
                                url = line.Split('>')[0].Replace("<a href=\"", "").Replace("\"", "");
                            }
                        }
                        if (line.Contains("<div class=\"chapter_count\"")) //Get chapter count
                        {
                            int index = line.IndexOf('>') + 1;
                            string text = "";
                            for (; index < line.Length; index++)
                            {
                                if (!numbers.Contains(line[index])) break;
                                text += line[index];
                            }
                            int.TryParse(text, out chapterCount);
                        }

                        if (chapterCount != 0 && !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(url))
                        {
                            MangaReaderManga manga = new MangaReaderManga();
                            manga.Title = title;
                            manga.ChapterCount = chapterCount;
                            manga.PageURL = "http://www.mangareader.net" + url;
                            manga.ImageURL = imgUrl;
                            manga.db = this;

                            chapterCount = 0;
                            title = "";
                            url = "";

                            mangas.Add(manga);

                            infoFound = false;
                            resultFound = false;
                            continue;
                        }
                    }
                }
            }

            //TODO Finish

            return mangas;
        }
    }
}
