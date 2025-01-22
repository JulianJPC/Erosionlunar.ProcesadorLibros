using MySqlX.XDevAPI.Common;
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

namespace Erosionlunar.ProcesadorLibros.Models.windowPPP2
{
    /// <summary>
    /// Interaction logic for numberInput.xaml
    /// </summary>
    public partial class numberInput : Window
    {
        public int? Result { get; private set; }
        public numberInput(string text)
        {
            textToShow.Text = text;
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumberTextBox.Text, out int number))
            {
                Result = number;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Numero no valido.", "Input invalido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
