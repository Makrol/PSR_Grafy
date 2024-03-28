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

namespace Grafy_serwer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        static TcpListener listener;
        const int port = 8888;
        static int clientCount = 0;
        static List<Thread> clientThreads = new List<Thread>();
        private Thread serverThred;
        public MainWindow()
        {
            InitializeComponent();
            counterTextBlock.Text = "Ilość połączonych klientów: 0";

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            serverThred = new Thread(new ParameterizedThreadStart(StartServer));
            serverThred.Start();
            startButton.IsEnabled = false;
        }
        void StartServer(object obj)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine("Serwer uruchomiony. Oczekiwanie na połączenia...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread newClientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThreads.Add(newClientThread);
                    newClientThread.Start(client);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
        void UpdateCounter()
        {
            clientCount++;
            Dispatcher.Invoke(() => { counterTextBlock.Text = "Ilość połączonych klientów: " + clientCount; });
        
        }
        void HandleClient(object obj)
        {
            
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                UpdateCounter();
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Odebrano: " + dataReceived);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }
    }
}