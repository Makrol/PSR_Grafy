using Grafy_serwer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace klient_server.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ClientListPage.xaml
    /// </summary>
    public partial class ClientListPage : Page
    {
        public static ObservableCollection<CliendRecord> ConnectedClientsRecords { get; } = new ObservableCollection<CliendRecord>();

        public ClientListPage()
        {
            InitializeComponent();
            listView.ItemsSource = ConnectedClientsRecords;
        }
        public static void resetActionCounters()
        {
            foreach (var item in ConnectedClientsRecords)
            {
                item.Count = 0;
                item.AvarageTime = 0;
                item.timeSum = 0;
            }
        }
    }
}
