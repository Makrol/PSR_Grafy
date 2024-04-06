using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Grafy_serwer
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public ObservableCollection<Record> Records { get; set; }
        public Window1()
        {
            InitializeComponent();
            Records = new ObservableCollection<Record>();
            listView.ItemsSource = Records;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Records.Add(new Record { IPAddress = "127.0.0.1", Port = 8888, Status = "aktywny" });
        }

        public class Record
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
            public string Status { get; set; }
        }
    }
}
