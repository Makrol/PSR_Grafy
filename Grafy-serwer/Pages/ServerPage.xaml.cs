using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
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
using System.Xml.Linq;

namespace Grafy_serwer.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        private TabControl tabControl;
        public ObservableCollection<Record> Records { get; set; }
        private static TcpListener listener;
        private const int serverPort = 8888;
        private static List<Thread> clientThreads = new List<Thread>();
        private int nodeInPackage =0;
        private int nodeCounter = 0;
        private int counter = 0;
        private Thread serverThread;
        private List<TcpClient> clients = new List<TcpClient> ();
        private List<NetworkStream> clientsStreams = new List<NetworkStream> ();

        public ServerPage()
        {
            InitializeComponent();
            Records = new ObservableCollection<Record>();
            listView.ItemsSource = Records;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                tabControl = mainWindow.mainTabControl;
                StartServer();
                
            }
        }

        private void CloseServer(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listener != null)
                {
                    // Zatrzymaj nasłuchiwanie serwera
                    listener.Stop();
                    Debug.WriteLine("Serwer został zatrzymany.");

                    // Zamknij wszystkie połączenia klientów
                    foreach (Thread clientThread in clientThreads)
                    {
                        clientThread.Abort(); // Przerwij wątek klienta
                    }
                    clientThreads.Clear(); // Wyczyść listę wątków klientów
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            tabControl.SelectedIndex = 1;
            Records.Clear();
            counter = 0;
        }
        private void StartServer()
        {
            //AdjacencyMatrix adjacencyMatrix = new AdjacencyMatrix(nodes.Count);
            //adjacencyMatrix.generateMatrix(nodes);
            serverThread = new Thread(new ParameterizedThreadStart(StartServer));
            serverThread.Start();
            tabControl.SelectedIndex = 3;
        }
        private void StartServer(object obj)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, serverPort);
                listener.Start();
                Debug.WriteLine("Serwer uruchomiony. Oczekiwanie na połączenia...");

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
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }


        private void HandleClient(object obj)
        {

            TcpClient client = (TcpClient)obj;
            clients.Add(client);
            NetworkStream stream = client.GetStream();
            clientsStreams.Add(stream);

            byte[] buffer = new byte[1024];
            int bytesRead;
            counter++;
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

            Dispatcher.Invoke(() => {
                clientCounter.Text = counter.ToString();
                Records.Add(new Record { IPAddress = clientIP, Port = clientPort, Status = "aktywny" });
            });
            try
            {
                while (true)
                {
                    // Sprawdź, czy klient jest nadal połączony
                    if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                    {
                        counter--;
                        Dispatcher.Invoke(() => {
                            clientCounter.Text = counter.ToString();
                            Record recordToRemove = Records.FirstOrDefault(record => record.IPAddress == clientIP && record.Port == clientPort);

                            if (recordToRemove != null)
                            {
                                Records.Remove(recordToRemove);
                            }
                        });
                        MessageBox.Show("Utracono połączenie z klientem o ip "+clientIP+" i porcie "+clientPort, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Jeśli klient został odłączony, przerwij pętlę
                        break;
                    }

                    // Odczytaj dane od klienta
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
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
                // Zamknij strumień i klienta
                stream.Close();
                client.Close();
                Dispatcher.Invoke(() =>
                {
                    tabControl.SelectedIndex = 0;
                });
                
            }
        }
        public class Record
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
            public string Status { get; set; }
        }

        private void StartCalculation(object sender, RoutedEventArgs e)
        {
            
            listener.Stop();
            calculationButton.IsEnabled=false;
            var sendableObjects = generateInitSendObjects();
            sendObjectsToAll(sendableObjects);

            
        }
        private List<SendObject> generateInitSendObjects()
        {
            var nodes = GraphEditorPage.getGrapfNodes();
            AdjacencyMatrix matrix = new AdjacencyMatrix(nodes.Count);
            matrix.generateMatrix(nodes);
            List<SendObject> messages = new List<SendObject>();
            nodeInPackage = nodes.Count / counter;
            nodeCounter = 0;
            for(int i=0;i<counter ;i++)
            {
                var tmp = new SendObject();
                tmp.matrix = matrix.matrix;
                for(int j=0;j<nodeInPackage;j++)
                {
                    if (nodeCounter > nodes.Count - 1)
                        break;
                    tmp.nodeIndexes.Add(nodeCounter);
                    nodeCounter++;
                }
                messages.Add(tmp);
                if((i+1)%nodeInPackage==0)
                {
                    continue;
                }
            }
            if(nodeCounter<nodes.Count)
            {
                while(nodeCounter!=nodes.Count)
                {
                    foreach(var mess in messages)
                    {
                        if (nodeCounter == nodes.Count)
                            break;
                        mess.nodeIndexes.Add(nodeCounter);
                        nodeCounter++;
                    }
                }
            }
            return messages;
        }
        private void sendObjectsToAll(List<SendObject> objectsList)
        {
            for(int i=0;i<objectsList.Count;i++)
            {
                clientsStreams[i].Write(JsonSerializer.SerializeToUtf8Bytes(objectsList[i]));
            }
        }
    }
}
