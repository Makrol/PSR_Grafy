using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grafy_serwer.Pages;
using Grafy_serwer;
using System.Text.Json;
using System.Windows.Threading;
using klient_server.Pages;
using System.Windows;

namespace klient_server.Threads
{
    public class HandleClientThread
    {
        private Thread thread;
        private TcpClient tcpClient;
        public NetworkStream stream;
        public string clientIP;
        public int clientPort;
        public HandleClientThread()
        {
            thread = new Thread(new ParameterizedThreadStart(threadTask));
        }
        public void start(TcpClient client)
        {
            thread.Start(client);
        }
        public void stop() {
            tcpClient.Close(); 
            stream.Close();
        }
        private void threadTask(object obj)
        {
            bool isClientEnd = false;
            tcpClient = (TcpClient)obj;
            stream = tcpClient.GetStream();

            clientIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            clientPort = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
            var newClientRecord = new CliendRecord { IPAddress = clientIP, Port = clientPort, Count = 0 };
            Application.Current.Dispatcher.Invoke(() =>
            {
                ClientListPage.ConnectedClientsRecords.Add(newClientRecord);
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

                    // Odczytywanie odpowiedzi
                    while (true)
                    {
                        isClientEnd = handleClientDisconect(tcpClient, stream, newClientRecord, clientIP, clientPort);
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

                    byte[] responseData = new byte[totalBytesRead];
                    Array.Copy(tmpResponseData, responseData, totalBytesRead);
                    string stringData = Encoding.UTF8.GetString(responseData);
                    string tmptest = Encoding.UTF8.GetString(responseData);
                    stringData = stringData[..^5];

                    if (!IsValidJson(stringData))
                        stringData += "}";
                    // Walidacja ciągu JSON przed deserializacją
                    if (IsValidJson(stringData))
                    {
                        try
                        {
                            ReturnObject recievedObject = JsonSerializer.Deserialize<ReturnObject>(stringData);
                            recievedObject.client = clientIP + ":" + clientPort;
                            ServerPage.addReturnObjectToList(recievedObject);

                            Application.Current.Dispatcher.Invoke(() => {
                                ServerPage.ResultsRecords.Add(new ResultRecord
                                {
                                    ClientName = clientIP + ":" + clientPort,
                                    RecieveDate = recievedObject.receiveTime.ToString("HH:mm:ss.fff"),
                                    BeginDate = recievedObject.beginTime.ToString("HH:mm:ss.fff"),
                                    EndDate = recievedObject.endTime.ToString("HH:mm:ss.fff"),
                                    result = recievedObject
                                });
                            });

                            Application.Current.Dispatcher.Invoke(() => {
                                CliendRecord clientRow = null;
                                foreach (var row in ClientListPage.ConnectedClientsRecords)
                                {
                                    if (row.IPAddress == newClientRecord.IPAddress && row.Port == newClientRecord.Port)
                                    {
                                        clientRow = row; break;
                                    }
                                }
                                if (clientRow != null)
                                {
                                    clientRow.Count++;
                                    clientRow.timeSum += (long)(recievedObject.endTime - recievedObject.beginTime).TotalMilliseconds;
                                    clientRow.AvarageTime = clientRow.timeSum / clientRow.Count;
                                }
                            });

                            ServerThread.generateAndSendNextPackage(stream);
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"Błąd deserializacji JSON: {jsonEx.Message}");
                            Console.WriteLine($"Ciąg JSON: {stringData}");
                            MessageBox.Show($"Błąd deserializacji JSON: {jsonEx.Message}", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"Nieprawidłowy ciąg JSON: {stringData}");
                        //MessageBox.Show($"Nieprawidłowy ciąg JSON: {stringData}", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                        MessageBox.Show("invalid JSON", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString(), "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                stream.Close();
                tcpClient.Close();
            }
        }

        private bool IsValidJson(string jsonString)
        {
            try
            {
                var json = JsonDocument.Parse(jsonString);
                return true;
            }
            catch (JsonException)
            {
                
                return false;
            }
        }

        private bool handleClientDisconect(TcpClient client, NetworkStream stream, CliendRecord newCliendRecord, string ip, int port)
        {
            if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
            {
               /// Dispatcher.Invoke(() =>
               // {
                    ClientListPage.ConnectedClientsRecords.Remove(newCliendRecord);
                    //clients.Remove(client);
                    //clientsStreams.Remove(stream);
                    //connectedClientsCounter--;
              //  });
                MessageBox.Show("Utracono połączenie z klientem o ip " + ip + " i porcie " + port, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            return false;
        }

    }
}
