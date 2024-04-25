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
        private AdjacencyMatrix matrix;
        private TabControl tabControl;
        public ObservableCollection<CliendRecord> ConnectedClientsRecords { get; set; }
        public ObservableCollection<ResultRecord> ResultsRecords { get; set; }
        private static TcpListener listener;
        private const int serverPort = 8888;
        private static List<Thread> clientThreads = new List<Thread>();
        private int nodeInPackage =0;
        private int nodeCounter = 0;
        private int connectedClientsCounter = 0;
        private Thread serverThread;
        private List<TcpClient> clients = new List<TcpClient> ();
        private List<NetworkStream> clientsStreams = new List<NetworkStream> ();
        private List<ReturnObject> returnObjectsList = new List<ReturnObject> ();
        private int nodeProgres = 0;
        static public int packageSize = -1;
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
            clientsStreams.Clear();
            returnObjectsList.Clear();
            ResultsRecords.Clear();
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
                    foreach (TcpClient client in clients)
                    {
                        client.Close();
                    }
                    foreach( var stream in clientsStreams)
                    {
                        stream.Close();
                    }
                    // Zamknij wszystkie połączenia klientów
                   /* foreach (Thread clientThread in clientThreads)
                    {
                        clientThread.Abort(); // Przerwij wątek klienta
                    }
                    clientThreads.Clear();*/ // Wyczyść listę wątków klientów
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            tabControl.SelectedIndex = 1;
            ConnectedClientsRecords.Clear();
            connectedClientsCounter = 0;
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
            bool isClientEnd = false;
            TcpClient client = (TcpClient)obj;
            clients.Add(client);
            NetworkStream stream = client.GetStream();
            clientsStreams.Add(stream);

            connectedClientsCounter++;
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            var newClientRecord = new CliendRecord { IPAddress = clientIP, Port = clientPort, Count = 0};
            Dispatcher.Invoke(() => {
                ConnectedClientsRecords.Add(newClientRecord);
            });
            try
            {
                while (true)
                {
                    int bufferSize = 1024;
                    int initBufferSize = bufferSize;
                    byte[] tmpResponseData = new byte[bufferSize];
                    int bytesRead = 0;
                    int totalBytesRead = 0;

                    while(true)
                    {
                        isClientEnd = handleClientDisconect(client, stream, newClientRecord, clientIP, clientPort);
                        if (isClientEnd)
                            break;
                        bytesRead = stream.Read(tmpResponseData, totalBytesRead, bufferSize);
                       // if (totalBytesRead == 0 && bytesRead == 0)
                         //   break;
                        totalBytesRead += bytesRead;
                        string tmpS = Encoding.UTF8.GetString(tmpResponseData);
                        if (tmpS.Contains("END"))
                        {
                            break;
                        }
                        initBufferSize += bufferSize;
                        Array.Resize(ref tmpResponseData, initBufferSize);
                    }

                    if (isClientEnd)
                        break;
                    byte[] responseData = new byte[totalBytesRead];
                    Array.Copy(tmpResponseData, responseData, totalBytesRead);
                    string stringData = Encoding.UTF8.GetString(responseData);
                    stringData = stringData[..^5];

                    ReturnObject recievedObject = JsonSerializer.Deserialize<ReturnObject>(stringData);
                    recievedObject.client = clientIP + ":" + clientPort;
                    returnObjectsList.Add(recievedObject);
                    Dispatcher.Invoke(() => {
                        ResultsRecords.Add(new ResultRecord
                        {
                            ClientName = clientIP + ":" + clientPort,
                            RecieveDate = recievedObject.receiveTime.ToString(),
                            BeginDate = recievedObject.beginTime.ToString(),
                            EndDate = recievedObject.endTime.ToString(),
                            result = recievedObject
                        });
                    });

                    //MessageBox.Show("Odebrano wyniki obliczeń od klienta w ilości "+recievedObject.results.Count, "Odebrani wyniki", MessageBoxButton.OK, MessageBoxImage.Information);
                    var objectToSend = generateNewSendObject(ServerPage.packageSize);
                    if (objectToSend != null)
                    {
                        Dispatcher.Invoke(() => {
                            ConnectedClientsRecords.First(row => row.IPAddress == newClientRecord.IPAddress && row.Port == newClientRecord.Port).Count++;
                        });
                      //  newClientRecord.Count++;

                        sendObject(objectToSend, stream);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            nodeProgres = 0;
                        });
                        break;
                    }
                        
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
        

        private void StartCalculation(object sender, RoutedEventArgs e)
        {
            if(connectedClientsCounter<=0)
            {
                MessageBox.Show("Nie można rozpocząć obliczeń ponieważ żaden klient nie połączył się z serwerem", "Brak klientów", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            GranulationWindow window = new GranulationWindow();
            window.ShowDialog();
            packageSize = window.granulationValue;
            if(packageSize != -1)
            {
                listener.Stop();
                calculationButton.IsEnabled=false;
                var sendableObjects = generateInitSendObjects(packageSize);
                sendObjectsToAll(sendableObjects);
                
            }
            

            
        }
        private SendObject generateNewSendObject(int nodeInPackage)
        {
            var nodes = GraphEditorPage.getGrapfNodes();
            if (nodeProgres >= nodes.Count)
                return null;
            var sendObject = new SendObject();
            sendObject.matrix = null;
            for (int j = 0; j < nodeInPackage; j++)
            {
                if (nodeProgres > nodes.Count - 1)
                    break;
                sendObject.nodeIndexes.Add(nodeProgres);
                nodeProgres++;
            }
            return sendObject;
        }
        private List<SendObject> generateInitSendObjects(int nodeInPackage)
        {
            var nodes = GraphEditorPage.getGrapfNodes();
            matrix = new AdjacencyMatrix(nodes.Count);
            matrix.generateMatrix(nodes);
            List<SendObject> messages = new List<SendObject>();
            for(int i=0;i<connectedClientsCounter ;i++)
            {
                var tmp = new SendObject();
                tmp.matrix = matrix.matrix;
                for(int j=0;j<nodeInPackage;j++)
                {
                    if (nodeProgres > nodes.Count - 1)
                        break;
                    tmp.nodeIndexes.Add(nodeProgres);
                    nodeProgres++;
                }
                messages.Add(tmp);
                if((i+1)%nodeInPackage==0)
                {
                    continue;
                }
            }
            return messages;
        }
        /*
         private List<SendObject> generateInitSendObjects(int packageSize)
        {
            var nodes = GraphEditorPage.getGrapfNodes();
            AdjacencyMatrix matrix = new AdjacencyMatrix(nodes.Count);
            matrix.generateMatrix(nodes);
            List<SendObject> messages = new List<SendObject>();
            nodeInPackage = nodes.Count / connectedClientsCounter;
            nodeCounter = 0;
            for(int i=0;i<connectedClientsCounter ;i++)
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
         */
        private void sendObjectsToAll(List<SendObject> objectsList)
        {          
            for (int i=0;i<objectsList.Count;i++)
            {
                ConnectedClientsRecords[i].Count++;
                sendObject(objectsList[i],clientsStreams[i]);
            }
        }
        private void sendObject(SendObject obj,NetworkStream stream)
        {
            var serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
            stream.Write(serializedData);
            stream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize("END")));
        }

        private void Show_record_Info(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ResultRecord item = button.DataContext as ResultRecord;
                if (item != null)
                {
                    AllResultsWindow window = new AllResultsWindow();
                    var tmpList = new List<ReturnObject>();
                    tmpList.Add(item.result);
                    window.returnObjectsList = tmpList;
                    window.ShowDialog();
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
        private bool handleClientDisconect(TcpClient client, NetworkStream stream, CliendRecord newCliendRecord,string ip,int port)
        {
            if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    ConnectedClientsRecords.Remove(newCliendRecord);
                    clients.Remove(client);
                    clientsStreams.Remove(stream);
                    connectedClientsCounter--;
                });
                MessageBox.Show("Utracono połączenie z klientem o ip " + ip + " i porcie " + port, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            return false;
        }
    }
}
