using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;
using klient_server.Threads;

namespace Grafy_serwer.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        public TcpClient client;
        public NetworkStream stream;
        public static TabControl tabControl;
        public static List<List<int>> matrix = null;
        private ClientThread clientT = new ClientThread();
        public ClientPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                tabControl = mainWindow.mainTabControl;
            }
            clientT.start();
        }

        private void Disconnect(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clientT.tcpClient != null)
                {
                    clientT.tcpClient.Close();
                }

                if (clientT.stream != null)
                {
                    clientT.stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas rozłączania z serwerem: " + ex.Message);
            }
            tabControl.SelectedIndex = 2;
        }

    }
}
