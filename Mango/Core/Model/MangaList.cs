using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Mango.Core.Model
{
    public class MangaList
    {
        public static List<Manga> List = new List<Manga>();

        public static void Load()
        {
            if (File.Exists("mangas/saved.dat"))
            {
                string[] lines = File.ReadAllLines("mangas/saved.dat");

                foreach (string line in lines)
                {
                    string[] data = line.Split(':');

                    Type mType = Assembly.GetExecutingAssembly().GetType(data[3]);
                    if (mType == null)
                        continue;

                    Manga m = (Manga)Activator.CreateInstance(mType, new object[] { });
                    m.Title = data[0];
                    m.CurrentPage = int.Parse(data[1]);
                    m.CurrentChapter = int.Parse(data[2]);
                    if (!m.IsDownloadComplete)
                    {
                        new Thread(new ThreadStart(delegate
                        {
                            m.Download();
                        })).Start();
                    }

                    List.Add(m);
                }
            }
        }

        public static void Save()
        {
            List<string> lines = new List<string>();
            foreach (Manga m in List)
            {
                lines.Add(m.Title + ":" + m.CurrentPage + ":" + m.CurrentChapter + ":" + m.GetType().ToString() + ":" + (m.DatabaseParent != null ? m.DatabaseParent.GetType().ToString() : " "));
            }
            if (!Directory.Exists("mangas"))
                Directory.CreateDirectory("mangas");

            File.WriteAllLines("mangas/saved.dat", lines.ToArray<string>());
        }
    }
}
