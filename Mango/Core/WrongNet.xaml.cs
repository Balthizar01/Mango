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
using Mango;
using System.Diagnostics;

namespace Mango.Core
{
    /// <summary>
    /// Interaction logic for WrongNet.xaml
    /// </summary>
    public partial class WrongNet : Window
    {
        public WrongNet()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dlBtn.Content = "Please wait..";
            dlBtn.IsEnabled = false;

            Process.Start("http://www.microsoft.com/en-us/download/details.aspx?id=30653");
            Application.Current.Shutdown();
        }
    }
}
