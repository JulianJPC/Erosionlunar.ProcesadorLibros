using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Erosionlunar.ProcesadorLibros
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void irAPreParteProceso_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is PreParteProcesos)
                {
                    window.Focus(); // Bring the existing window to the front.
                    return;
                }
            }

            // If no instance exists, create and show a new one.
            PreParteProcesos otherWindow = new PreParteProcesos();
            otherWindow.Show();
            this.Close(); // Close the current window if desired.
        }
    }
}