using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grafy_serwer.Modals;
using Grafy_serwer;
using System.Windows;
using Grafy_serwer.Pages;
using System.Windows.Media;
using System.Text.Json;
using klient_server.Pages;
using System.IO;
using klient_server.Dto;

namespace klient_server.Threads
{
    public class ServerThread
    {
        private Thread thread;
        private TcpListener listener;
        private const int serverPort = 8888;
        private static List<HandleClientThread> handleClientList = new List<HandleClientThread>();
        private static int connectedClientsCounter = 0;
        public static int packageSize = -1;
        static object lockPackageGenerator = new object();
        public static AdjacencyMatrix matrix;
        public static int nodeProgres = 0;
        public ServerThread() 
        {
            thread = new Thread(new ParameterizedThreadStart(threadTask));
        }
        public void start()
        {
            thread.Start();
        }
        public void stop()
        {
            listener.Stop();
            foreach (var item in handleClientList)
            {
                item.stop();
            }
            handleClientList.Clear();
        }
        private void threadTask(object obj)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, serverPort);
                listener.Start();
                Debug.WriteLine("Serwer uruchomiony. Oczekiwanie na połączenia...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    HandleClientThread handleClientThread = new HandleClientThread();
                    handleClientList.Add(handleClientThread);
                    handleClientThread.start(client);
                    connectedClientsCounter++;

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
        public static void StartCalculation()
        {
            nodeProgres = 0;
            if (connectedClientsCounter <= 0)
            {
                MessageBox.Show("Nie można rozpocząć obliczeń ponieważ żaden klient nie połączył się z serwerem", "Brak klientów", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            GranulationWindow window = new GranulationWindow();
            window.ShowDialog();
            packageSize = window.granulationValue;
            if (packageSize != -1)
            {
               // listener.Stop();
                //calculationButton.IsEnabled = false;
                List<SendObject> sendableObjects = null;
                lock (lockPackageGenerator)
                {
                    sendableObjects = generateInitSendObjects(packageSize);
                }

                sendObjectsToAll(sendableObjects);

            }



        }
        private static List<SendObject> generateInitSendObjects(int nodeInPackage)
        {
            var nodes = GraphEditorPage.getGrapfNodes();
            matrix = new AdjacencyMatrix(nodes.Count);
            matrix.generateMatrix(nodes);
            List<SendObject> messages = new List<SendObject>();
            for (int i = 0; i < connectedClientsCounter; i++)
            {
                var tmp = new SendObject();
                tmp.matrix = matrix.matrix;
                for (int j = 0; j < nodeInPackage; j++)
                {
                    if (nodeProgres > nodes.Count - 1)
                        break;
                    tmp.nodeIndexes.Add(nodeProgres);
                    nodeProgres++;
                }
                messages.Add(tmp);
                if ((i + 1) % nodeInPackage == 0)
                {
                    continue;
                }
            }
            return messages;
        }
        private static void sendObjectsToAll(List<SendObject> objectsList)
        {
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < objectsList.Count; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(initSendThread));
                thread.Start(new ObjectAndStreamDto(objectsList[i], handleClientList[i].stream) );
            }
        }
        private static void initSendThread(object obj)
        {
            var dtoObject = (ObjectAndStreamDto)obj;
            var serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dtoObject.obj));
            dtoObject.stream.Write(serializedData);
            dtoObject.stream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize("END")));
        }
        private static void sendObject(SendObject obj, NetworkStream stream)
        {
            var serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
            stream.Write(serializedData);
            stream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize("END")));
        }
        public static void generateAndSendNextPackage(NetworkStream stream)
        {
            SendObject objectToSend;
            lock (lockPackageGenerator)
            {
                objectToSend = generateNewSendObject(packageSize);
            }
            if(objectToSend==null)
            {
                MessageBox.Show("", "Koniec", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                ServerPage.unlockButtons();
            }
            else
                sendObject(objectToSend, stream);
        }
        private static SendObject generateNewSendObject(int nodeInPackage)
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
    }
}
