using Grafy_serwer.Pages;
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

namespace Grafy_serwer.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        private TabControl tabControl;
        public WelcomePage()
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

        private void SelectServer(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
            ServerPage.serverT.start();
        }
        private void SelectClient(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
        }
    }
}
