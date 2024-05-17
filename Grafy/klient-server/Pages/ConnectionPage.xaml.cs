using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Grafy_serwer.Pages
{
    public partial class ConnectionPage : Page
    {
        private Thread clientThread;
        private TabControl tabControl;

        public ConnectionPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Pobierz główne okno, w którym jest zawarta strona ConnectionPage.xaml
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                // Uzyskaj dostęp do mainTabControl z poziomu MainWindow
                tabControl = mainWindow.mainTabControl;
            }
        }

        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.clientIp = ipAddress.Text;
                mainWindow.clientPort = int.Parse(port.Text);
            });
            tabControl.SelectedIndex = 4;
        }

        private void CancelConnection(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
        }
    }
}
