using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;

namespace Mango.Core.Database.Impl
{
    public class MangaHereDatabase : MangaDatabase
    {

        public string Name
        {
            get { return "MangaHere"; }
        }

        public string SiteURL
        {
            get { return "http://mangahere.com/"; }
        }

        public async Task<List<Model.Manga>> Search(string keyword)
        {
            List<Model.Manga> list = new List<Model.Manga>();
            string url = System.Web.HttpUtility.UrlEncode(keyword);
            url = "http://www.mangahere.com/search.php?name=" + url;
            string result = null;
            try
            {
                using (WebClient client = new WebClient())
                {
                    result = await client.DownloadStringTaskAsync(url);
                }
            }
            catch
            {
                return list;
            }

            if (String.IsNullOrWhiteSpace(result))
                return list;

            string[] lines = result.Split('\n');
            bool found = false;
            int chapterCount = 0;
            string title = "";
            string murl = "";
            bool volume = false;
            string imgUrl = "";
            foreach (string line in lines)
            {
                string l = line.Trim();
                if (!found)
                {
                    if (l == "<dt>")
                    {
                        found = true;
                        continue;
                    }
                }
                else
                {
                    if (l.StartsWith("<a")) //Name, link, IMAGE AND SERIES INFO HOLY SHIT!
                    {
                        bool start = false;
                        for (int i = 0; i < l.Length; i++)
                        {
                            if (!start && l[i] == '"')
                            {
                                start = true;
                                continue;
                            }
                            else if (start && l[i] == '"')
                            {
                                start = false;
                                break;
                            }

                            if (start)
                            {
                                murl += l[i];
                            }
                        }

                        try
                        {
                            string tr;
                            using (WebClient temp = new WebClient())
                            {
                                tr = await temp.DownloadStringTaskAsync(murl);
                            }
                            if (!String.IsNullOrWhiteSpace(tr))
                            {
                                volume = tr.Contains("Vol");
                                bool uwotm8 = false;
                                string[] lll = tr.Split('\n');
                                foreach (string wat in lll)
                                {
                                    if (!uwotm8)
                                    {
                                        if (wat.Contains("<div class=\"manga_detail_top clearfix\">"))
                                        {
                                            uwotm8 = true;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (wat.Trim().StartsWith("<img"))
                                        {
                                            start = false;
                                            int iistart = wat.IndexOf("<img src=");
                                            for (int i = iistart; i < wat.Length; i++)
                                            {
                                                if (!start && wat[i] == '"')
                                                {
                                                    start = true;
                                                    continue;
                                                }
                                                else if (start && wat[i] == '"')
                                                {
                                                    start = false;
                                                    break;
                                                }
                                                else if (start)
                                                {
                                                    imgUrl += wat[i];
                                                }
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }

                        int istart = l.IndexOf("rel=");
                        start = false;
                        for (int i = istart; i < l.Length; i++)
                        {
                            if (!start && l[i] == '"')
                            {
                                start = true;
                                continue;
                            }
                            else if (start && l[i] == '"')
                            {
                                start = false;
                                break;
                            }

                            if (start)
                            {
                                title += l[i];
                            }
                        }

                        string post = "name=" + System.Web.HttpUtility.UrlEncode(title);

                        WebRequest request = WebRequest.Create("http://www.mangahere.com/ajax/series.php");
                        ((HttpWebRequest)request).UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.116 Safari/537.36";
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        ((HttpWebRequest)request).Referer = url;
                        ((HttpWebRequest)request).Accept = "application/json, text/javascript, */*; q=0.01";

                        byte[] encoded = Encoding.ASCII.GetBytes(post);
                        request.ContentLength = encoded.Length;
                        Stream stream = request.GetRequestStream();
                        stream.Write(encoded, 0, encoded.Length);
                        stream.Close();

                        WebResponse response = request.GetResponse();
                        Stream rs = response.GetResponseStream();

                        byte[] finalData = new byte[1024];
                        MemoryStream ms = new MemoryStream();
                        int read = 0;
                        do
                        {
                            read = await rs.ReadAsync(finalData, 0, 1024);
                            ms.Write(finalData, 0, read);
                        } while (read > 0);
                        finalData = ms.ToArray();
                        ms.Close();


                        rs.Close();
                        string json = Encoding.ASCII.GetString(finalData);
                        if (String.IsNullOrWhiteSpace(json))
                        {
                            MangaHereManga mmanga = new MangaHereManga();
                            mmanga.Title = title;
                            mmanga.db = this;
                            mmanga.ImageURL = imgUrl;
                            mmanga.PageURL = murl;
                            list.Add(mmanga);
                            found = false;
                            continue;
                        }
                        json = json.Replace("\0", "");
                        json = json.Replace("[", "").Replace("]", "").Replace("\"", "").Trim().Replace("\\/", "/").Replace("\\", "\"");
                        string[] results = json.Split(',');

                        MangaHereManga manga = new MangaHereManga();
                        manga.Title = results[0];
                        manga.SetExtraData("Genre", results[4]);
                        manga.SetExtraData("Author", results[5]);
                        manga.SetExtraData("Released", results[6]);
                        manga.SetExtraData("Rank", results[7]);
                        manga.SetExtraData("Summary", results[8]);
                        manga.db = this;
                        manga.ImageURL = imgUrl;
                        manga.PageURL = murl;
                        manga.usesVolumes = volume;
                        if (manga.usesVolumes)
                            manga.CurrentVolume = 1;
                        list.Add(manga);
                        found = false;

                        title = "";
                        imgUrl = "";
                        murl = "";
                        continue;
                    }
                }
            }

            return list;
        }
    }
}
