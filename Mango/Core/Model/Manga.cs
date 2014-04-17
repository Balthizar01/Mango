using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mango.Core.Database;
using System.Threading;

namespace Mango.Core.Model
{
    public abstract class Manga
    {
        private Dictionary<string, object> extras = new Dictionary<string, object>();
        public abstract bool HasNext();

        public abstract bool HasPrevious();

        public abstract Task<bool> Next();

        public abstract Task<bool> Previous();

        public abstract void PrepareDisplay();

        public abstract void Display(System.Windows.Controls.Grid displayGrid);

        public abstract System.Windows.Media.ImageSource GetCover();

        public abstract void Download();

        public abstract void CancelDownload(Thread downloadThread);

        public void CancelDownload()
        {
            CancelDownload(null);
        }

        public object GetExtraData(string key)
        {
            if (!HasExtraData(key))
                return null;
            return extras[key];
        }

        public bool HasExtraData(string key)
        {
            return extras.ContainsKey(key);
        }

        public void SetExtraData(string key, object obj)
        {
            if (HasExtraData(key))
                extras[key] = obj;
            else
                extras.Add(key, obj);
        }

        public abstract MangaDatabase DatabaseParent
        {
            get;
        }

        public abstract int CurrentPage
        {
            get;
            internal set;
        }

        public abstract int CurrentChapter
        {
            get;
            internal set;
        }

        public abstract int CurrentVolume
        {
            get;
            internal set;
        }

        public abstract bool UsesVolumes
        {
            get;
            internal set;
        }

        public abstract bool IsDownloaded
        {
            get;
        }

        public abstract bool IsDownloading
        {
            get;
        }

        public abstract bool IsDownloadComplete
        {
            get;
        }
        
        public string Title
        {
            get;
            set;
        }

        public int ChapterCount
        {
            get;
            set;
        }
    }
}
