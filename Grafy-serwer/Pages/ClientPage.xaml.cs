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

namespace Grafy_serwer.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        public TcpClient client;
        public NetworkStream stream;
        private TabControl tabControl;
        private Thread clientThread;
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
            startConnection();
        }

        private void Disconnect(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            try
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (client != null)
                {
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas rozłączania z serwerem: " + ex.Message);
            }
            tabControl.SelectedIndex = 2;
        }

        private void startConnection()
        {
            clientThread = new Thread(new ParameterizedThreadStart(StartClientThread));
            clientThread.Start();
        }

        private void StartClientThread(object obj)
        {
            try
            {
                string serverIP = "";
                int ipPort = 0;
                MainWindow mainWindow = null; // Zainicjowanie zmiennej poza blokiem Dispatcher.Invoke

                // Użycie Dispatcher.Invoke do odczytu wartości z TextBox
                Dispatcher.Invoke(() =>
                {
                    mainWindow = Application.Current.MainWindow as MainWindow;
                    serverIP = mainWindow.clientIp;
                    ipPort = mainWindow.clientPort;
                });

                // Sprawdź, czy mainWindow nie jest null
                if (mainWindow != null)
                {
                    // Utwórz obiekt TcpClient i NetworkStream
                    client = new TcpClient(serverIP, ipPort);
                    stream = client.GetStream();

                    while (true)
                    {
                        Dispatcher.Invoke(()=> { clientStstus.Text = "Status: oczekuje na zadanie"; });
                        byte[] responseData = new byte[3024];
                        int bytesRead = stream.Read(responseData, 0, responseData.Length);
                        MessageBox.Show("KLient otrzymał pakiet danych", "Otrzymano dane", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
                else
                {
                    Console.WriteLine("mainWindow is null");
                }
            }
            catch (IOException ex) when ((ex.InnerException as SocketException)?.SocketErrorCode == SocketError.ConnectionReset)
            {
                Dispatcher.Invoke(() => { clientStstus.Text = "Status: rozłączony"; });
                MessageBox.Show("Serwer zresetował połączenie.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => { clientStstus.Text = "Status: rozłączony"; });
                Console.WriteLine("Błąd: " + ex.Message);
            }
            tabControl.SelectedIndex = 0;
        }
    }
}
