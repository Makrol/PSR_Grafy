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

namespace Grafy_klient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread clientThread;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void StartClientThread(object obj)
        {
            try
            {
                string serverIP = "127.0.0.1";
                int port = 8888;

                TcpClient client = new TcpClient(serverIP, port);

                NetworkStream stream = client.GetStream();

                while (true)
                {
                    Dispatcher.Invoke(() => { connectionInfo.Text = "Status połączenie: połączono"; });

                    byte[] responseData = new byte[1024];
                    int bytesRead = stream.Read(responseData, 0, responseData.Length);
                }

                // Zamknięcie połączenia
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd: " + ex.Message);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clientThread = new Thread(new ParameterizedThreadStart(StartClientThread)); 
            clientThread.Start();
            connectButton.IsEnabled = false;
        }
    }
}