using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Grafy_serwer.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private TabControl tabControl;
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
        }

        private void Disconnect(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            try
            {
                if (mainWindow.Stream != null)
                {
                    mainWindow.Stream.Close();
                }

                if (mainWindow.Client != null)
                {
                    mainWindow.Client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas rozłączania z serwerem: " + ex.Message);
            }
            tabControl.SelectedIndex = 2;
        }

    }
}
