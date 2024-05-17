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
        public static List<List<int>> matrix = null;
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
            bool isClientEnd = false;
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
                        int bufferSize = 1024;
                        int initBufferSize = bufferSize;
                        ReturnObject returnObject = new ReturnObject();
                        Dispatcher.Invoke(()=> { clientStstus.Text = "Status: oczekuje na zadanie"; });
                        byte[] tmpResponseData = new byte[bufferSize];
                        int bytesRead=0;
                        int totalBytesRead=0;
                        
                        
                        while(true)
                        {
                            isClientEnd=handleServertDisconect(client);
                            if (isClientEnd)
                                break;
                            bytesRead = stream.Read(tmpResponseData, totalBytesRead, bufferSize);
                            totalBytesRead+= bytesRead;
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
                        returnObject.receiveTime = DateTime.Now;
                        byte[] responseData = new byte[totalBytesRead];
                        Array.Copy(tmpResponseData, responseData, totalBytesRead);
                        string stringData = Encoding.UTF8.GetString(responseData);
                        stringData = stringData[..^5];
                        SendObject recievedObject = JsonSerializer.Deserialize<SendObject>(stringData);
                        if (recievedObject == null)
                            continue;
                        if (ClientPage.matrix == null)
                            ClientPage.matrix = recievedObject.matrix;
                       // MessageBox.Show("KLient otrzymał pakiet danych", "Otrzymano dane", MessageBoxButton.OK, MessageBoxImage.Information);
                        Dispatcher.Invoke(() => { clientStstus.Text = "Status: wykonuje obliczenia"; });

                        returnObject.beginTime = DateTime.Now;
                        foreach (var index in recievedObject.nodeIndexes)
                        {
                            returnObject.results.Add(Dijkstra.determineSolution(ClientPage.matrix, index, ClientPage.matrix.Count));
                        }
                        returnObject.endTime = DateTime.Now;

                        //zwracanie do serwera
                        var tmpData = JsonSerializer.Serialize(returnObject);
                        stream.Write(Encoding.UTF8.GetBytes(tmpData));
                        stream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize("END")));

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
            Dispatcher.Invoke(() => { tabControl.SelectedIndex = 0; });
            ClientPage.matrix = null;

            
        }
        private bool handleServertDisconect(TcpClient client)
        {
            if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    /*ConnectedClientsRecords.Remove(newCliendRecord);
                    clients.Remove(client);
                    clientsStreams.Remove(stream);
                    connectedClientsCounter--;*/
                });
                MessageBox.Show("Utracono połączenie z serwerem","Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            return false;
        }
    }
}
