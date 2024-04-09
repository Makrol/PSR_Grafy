using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Grafy_serwer.Window1;

namespace Grafy_serwer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {


        public string clientIp { set; get; }
        public int clientPort {  set; get; }
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}