using Grafy_serwer.Modals;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Grafy_serwer.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        private TabControl tabControl;
        public ObservableCollection<CliendRecord> ConnectedClientsRecords { get; set; }
        public ObservableCollection<ResultRecord> ResultsRecords { get; set; }
        private static TcpListener listener;
        private const int serverPort = 8888;
        private static List<Thread> clientThreads = new List<Thread>();
        private int nodeInPackage =0;
        private int nodeCounter = 0;
        private int counter = 0;
        private Thread serverThread;
        private List<TcpClient> clients = new List<TcpClient> ();
        private List<NetworkStream> clientsStreams = new List<NetworkStream> ();
        private List<ReturnObject> returnObjectsList = new List<ReturnObject> ();

        public ServerPage()
        {
            InitializeComponent();
            ConnectedClientsRecords = new ObservableCollection<CliendRecord>();
            ResultsRecords = new ObservableCollection<ResultRecord>();
            listView.ItemsSource = ConnectedClientsRecords;
            resultListView.ItemsSource = ResultsRecords;

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
            calculationButton.IsEnabled = true;
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
            ConnectedClientsRecords.Clear();
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

            counter++;
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

            Dispatcher.Invoke(() => {
                ConnectedClientsRecords.Add(new CliendRecord { IPAddress = clientIP, Port = clientPort, Status = "aktywny" });
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
                            CliendRecord recordToRemove = ConnectedClientsRecords.FirstOrDefault(record => record.IPAddress == clientIP && record.Port == clientPort);

                            if (recordToRemove != null)
                            {
                                ConnectedClientsRecords.Remove(recordToRemove);
                            }
                        });
                        MessageBox.Show("Utracono połączenie z klientem o ip "+clientIP+" i porcie "+clientPort, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Jeśli klient został odłączony, przerwij pętlę
                        break;
                    }
                    int bufferSize = 1024;
                    int initBufferSize = bufferSize;
                    byte[] tmpResponseData = new byte[bufferSize];
                    int bytesRead = 0;
                    int totalBytesRead = 0;

                    while(true)
                    {
                        bytesRead = stream.Read(tmpResponseData, totalBytesRead, bufferSize);
                        totalBytesRead += bytesRead;
                        string tmpS = Encoding.UTF8.GetString(tmpResponseData);
                        if (tmpS.Contains("END"))
                        {
                            break;
                        }
                        initBufferSize += bufferSize;
                        Array.Resize(ref tmpResponseData, initBufferSize);
                    }

                   /* byte[] tmpResponseData = new byte[3024];
                    int bytesRead = stream.Read(tmpResponseData, 0, tmpResponseData.Length);
                    byte[] responseData = new byte[bytesRead];
                    Array.Copy(tmpResponseData, responseData, bytesRead);*/

                    byte[] responseData = new byte[totalBytesRead];
                    Array.Copy(tmpResponseData, responseData, totalBytesRead);
                    string stringData = Encoding.UTF8.GetString(responseData);
                    stringData = stringData[..^5];

                    ReturnObject recievedObject = JsonSerializer.Deserialize<ReturnObject>(stringData);
                    recievedObject.client = clientIP + ":" + clientPort;
                    returnObjectsList.Add(recievedObject);
                    Dispatcher.Invoke(() => {
                        ResultsRecords.Add(new ResultRecord { 
                            ClientName = clientIP+":"+clientPort, 
                            RecieveDate = recievedObject.receiveTime.ToString(), 
                            BeginDate = recievedObject.beginTime.ToString(),
                            EndDate = recievedObject.endTime.ToString(),
                            result = recievedObject});
                    });
                    MessageBox.Show("Odebrano wyniki obliczeń od klienta w ilości "+recievedObject.results.Count, "Odebrani wyniki", MessageBoxButton.OK, MessageBoxImage.Information);

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
            for (int i=0;i<objectsList.Count;i++)
            {
                var tmpData = JsonSerializer.Serialize(objectsList[i]);
                var test = Encoding.UTF8.GetBytes(tmpData);
                clientsStreams[i].Write(test);

                

                clientsStreams[i].Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize("END")));

            }
        }

        private void Show_record_Info(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ResultRecord item = button.DataContext as ResultRecord;
                if (item != null)
                {
                    // Tutaj możesz wykorzystać obiekt 'item', na podstawie którego został wygenerowany rekord
                }
            }
        }

        private void Show_All_Results(object sender, RoutedEventArgs e)
        {
             AllResultsWindow window = new AllResultsWindow();

            window.returnObjectsList = returnObjectsList;
            window.ShowDialog();
        }
    }
}
