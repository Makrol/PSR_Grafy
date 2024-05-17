using Grafy_serwer.Modals;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Threading;
using System;
using klient_server.Threads;
using klient_server.Pages;
namespace Grafy_serwer.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ServerPage.xaml
    /// </summary>
    public partial class ServerPage : Page
    {
        private TabControl tabControl;
        public ObservableCollection<CliendRecord> ConnectedClientsRecords { get; set; }
        public static ObservableCollection<ResultRecord> ResultsRecords { get; set; }
        private List<NetworkStream> clientsStreams = new List<NetworkStream> ();
        private static List<ReturnObject> returnObjectsList = new List<ReturnObject> ();
        static public int packageSize = -1;
        public static ServerThread serverT= new ServerThread();
        private static Button runButton;
        private static Button backButton;
        public ServerPage()
        {
            InitializeComponent();
            ConnectedClientsRecords = new ObservableCollection<CliendRecord>();
            ResultsRecords = new ObservableCollection<ResultRecord>();
            //listView.ItemsSource = ConnectedClientsRecords;
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
                
            }
        }

        private void CloseServer(object sender, RoutedEventArgs e)
        {
          
        }
        private void StartServer()
        {
        }
      

        private void StartCalculation(object sender, RoutedEventArgs e)
        {
            ResultsRecords.Clear();
            ServerThread.StartCalculation();
            ClientListPage.resetActionCounters();
            calculationButton.IsEnabled = false;
            calculationButton.Content = "Obliczenia trwają";
            ServerBackButton.IsEnabled = false;
            runButton = calculationButton;
            backButton = ServerBackButton;
        }
        public static void unlockButtons()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                runButton.IsEnabled = true;
                runButton.Content = "Rozpocznij";
                backButton.IsEnabled = true;
            });
           
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
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Pliki CSV (*.csv)|*.csv";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine("Clients,RecieveDate,BegineDate,EndDate");
                        foreach (var record in ResultsRecords)
                        {
                            string line = $"{record.ClientName},{record.RecieveDate},{record.BeginDate},{record.EndDate}";
                            writer.WriteLine(line);
                        }

                        MessageBox.Show("Dane zostały zapisane do pliku CSV.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas zapisywania danych: {ex.Message}");
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
        }
        public static void addReturnObjectToList(ReturnObject returnObject)
        {
            returnObjectsList.Add(returnObject);
        }
    }
}
