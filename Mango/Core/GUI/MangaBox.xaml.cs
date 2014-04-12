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

namespace Mango.Core.GUI
{
    /// <summary>
    /// Interaction logic for MangaBox.xaml
    /// </summary>
    public partial class MangaBox : Window
    {
        public string MangaTitle
        {
            get
            {
                return (string)Title.Content;
            }
            set
            {
                SetTitle(value);
            }
        }

        public ImageSource MangaCover
        {
            get
            {
                return Cover.Source;
            }
            set
            {
                SetCover(value);
            }
        }

        public string DatabaseText
        {
            get
            {
                return (string)dbLabel.Content;
            }
            set
            {
                SetDB(value);
            }
        }

        public MangaBox()
        {
            InitializeComponent();
        }

        private void SetTitle(string title)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                Title.Content = title;
            }));
        }

        private void SetCover(ImageSource img)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                Cover.Source = img;
            }));
        }

        private void SetDB(string text)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                dbLabel.Content = text;
            }));
        }
    }
}
