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
        private TcpClient client;
        private NetworkStream stream;
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
            clientThread = new Thread(new ParameterizedThreadStart(StartClientThread));
            clientThread.Start();
            tabControl.SelectedIndex = 4;
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
                    serverIP = ipAddress.Text;
                    ipPort = int.Parse(port.Text);
                });

                // Sprawdź, czy mainWindow nie jest null
                if (mainWindow != null)
                {
                    // Utwórz obiekt TcpClient i NetworkStream
                    TcpClient client = new TcpClient(serverIP, ipPort);
                    NetworkStream stream = client.GetStream();

                    // Przypisz klienta i strumień do mainWindow
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.Client = client;
                        mainWindow.Stream = stream;
                    });

                    while (true)
                    {
                        // Odczyt danych z serwera
                        byte[] responseData = new byte[1024];
                        int bytesRead = mainWindow.Stream.Read(responseData, 0, responseData.Length);
                    }
                }
                else
                {
                    Console.WriteLine("mainWindow is null");
                }
            }
            catch (IOException ex) when ((ex.InnerException as SocketException)?.SocketErrorCode == SocketError.ConnectionReset)
            {
                MessageBox.Show("Serwer zresetował połączenie.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd: " + ex.Message);
            }
            tabControl.SelectedIndex = 0;
        }

        private void CancelConnection(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
        }
    }
}
