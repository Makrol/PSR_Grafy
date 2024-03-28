using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafy
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Ellipse selectedPoint = null;
        private List<Ellipse> points = new List<Ellipse>();
        private int mode = 1;
        private List<Line> lines = new List<Line>();
        private List<Point> linePoint = new List<Point>();


        public MainWindow()
        {
            //InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ustawienie adresu IP i portu serwera
                string serverIP = "127.0.0.1"; // Adres IP serwera
                int port = 8888; // Port serwera

                // Utworzenie klienta TCP
                TcpClient client = new TcpClient(serverIP, port);
                Console.WriteLine("Nawiązano połączenie z serwerem.");

                // Utworzenie strumienia sieciowego do komunikacji z serwerem
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    // Wprowadzenie wiadomości od użytkownika
                    Console.Write("Wprowadź wiadomość: ");
                    string message = Console.ReadLine();

                    // Konwersja wiadomości na tablicę bajtów
                    byte[] dataToSend = Encoding.ASCII.GetBytes(message);

                    // Wysłanie wiadomości do serwera
                    stream.Write(dataToSend, 0, dataToSend.Length);

                    // Odebranie odpowiedzi od serwera
                    byte[] responseData = new byte[1024];
                    int bytesRead = stream.Read(responseData, 0, responseData.Length);
                    string responseMessage = Encoding.ASCII.GetString(responseData, 0, bytesRead);
                    Console.WriteLine("Odpowiedź od serwera: " + responseMessage);
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
    }
}
