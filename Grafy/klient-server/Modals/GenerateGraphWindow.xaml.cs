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
using System.Windows.Shapes;


namespace Grafy_serwer.Modals
{
    public partial class GenerateGraphWindow : Window
    {

        public GenerateGraphWindow()
        {
            InitializeComponent();

        }
       
        // Metoda obsługująca zdarzenie wprowadzania tekstu, aby akceptować tylko cyfry
        private void NumericInputOnly(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");

            e.Handled = regex.IsMatch(e.Text);
        }
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {

            this.Close(); // Zamyka okno
        }
    }
}
