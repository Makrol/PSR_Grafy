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
    /// <summary>
    /// Logika interakcji dla klasy GranulationWindow.xaml
    /// </summary>
    public partial class GranulationWindow : Window
    {
        public int granulationValue = -1;
        public GranulationWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            granulationValue = int.Parse(granulation.Text);
            this.Close();
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            granulationValue = -1;
            this.Close();
        }
        private void NumericInputOnly(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");

            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
