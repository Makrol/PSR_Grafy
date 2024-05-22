using Grafy_serwer.Pages;
using Grafy_serwer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

namespace klient_server.Threads
{
    public class ClientThread
    {
        private Thread thread;
        public TcpClient tcpClient;
        public NetworkStream stream;

        public ClientThread()
        {
            thread = new Thread(new ParameterizedThreadStart(threadTask));
        }
        public void start()
        {
            thread.Start();
        }
        public void stop()
        {

        }
        private void threadTask(object obj)
        {
            bool isClientEnd = false;
            try
            {
                string serverIP = "";
                int ipPort = 0;
                MainWindow mainWindow = null; // Zainicjowanie zmiennej poza blokiem Dispatcher.Invoke

                // Użycie Dispatcher.Invoke do odczytu wartości z TextBox
                Application.Current.Dispatcher.Invoke(() =>
                {
                    mainWindow = Application.Current.MainWindow as MainWindow;
                    serverIP = mainWindow.clientIp;
                    ipPort = mainWindow.clientPort;
                });

                // Sprawdź, czy mainWindow nie jest null
                if (mainWindow != null)
                {
                    // Utwórz obiekt TcpClient i NetworkStream
                    tcpClient = new TcpClient(serverIP, ipPort);
                    stream = tcpClient.GetStream();

                    while (true)
                    {
                        int bufferSize = 1024;
                        int initBufferSize = bufferSize;
                        ReturnObject returnObject = new ReturnObject();
                       // Application.Current.Dispatcher.Invoke(() => { clientStstus.Text = "Status: oczekuje na zadanie"; });
                        byte[] tmpResponseData = new byte[bufferSize];
                        int bytesRead = 0;
                        int totalBytesRead = 0;


                        while (true)
                        {
                            isClientEnd = handleServertDisconect(tcpClient);
                            if (isClientEnd)
                                break;
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
                       // Dispatcher.Invoke(() => { clientStstus.Text = "Status: wykonuje obliczenia"; });

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
                //Dispatcher.Invoke(() => { clientStstus.Text = "Status: rozłączony"; });
                //MessageBox.Show("Serwer zresetował połączenie.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                MessageBox.Show($"Serwer zresetował połączenie. Szczegóły: {ex.Message}", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);

                var innerExceptionMessage = ex.InnerException?.Message ?? "Brak dodatkowych informacji";
                MessageBox.Show($"Serwer zresetował połączenie. Szczegóły: {ex.Message}\nWewnętrzny wyjątek: {innerExceptionMessage}", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                //Dispatcher.Invoke(() => { clientStstus.Text = "Status: rozłączony"; });
                Console.WriteLine("Błąd: " + ex.Message);
            }
            //Dispatcher.Invoke(() => { tabControl.SelectedIndex = 0; });
            ClientPage.matrix = null;
        }
        private bool handleServertDisconect(TcpClient client)
        {
            if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    /*ConnectedClientsRecords.Remove(newCliendRecord);
                    clients.Remove(client);
                    clientsStreams.Remove(stream);
                    connectedClientsCounter--;*/
                });
                MessageBox.Show("Utracono połączenie z serwerem", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            return false;
        }
    }
}
