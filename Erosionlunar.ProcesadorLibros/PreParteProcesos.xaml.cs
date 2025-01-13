using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models.windowPPP;
using Mysqlx.Cursor;
using Mysqlx.Prepare;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace Erosionlunar.ProcesadorLibros
{
    /// <summary>
    /// Interaction logic for PreParteProcesos.xaml
    /// </summary>
    public partial class PreParteProcesos : Window
    {
        private readonly DBConector _context;
        private readonly string pathProcesos;
        public PreParteProcesos()
        {
            _context = new DBConector();
            pathProcesos = getPathProcesos();
            var rawPrePartes = getPrePartesSinParte();
            getArchivosPrePartes(rawPrePartes);
            var objectsList = createListPPS(rawPrePartes);

            InitializeComponent();
            foreach (PreParteSeleccion onePP in objectsList)
            {
                PrePartesToUse.Items.Add(onePP.NumeroP);
            }
            DG1.ItemsSource = objectsList;
        }
        /// <summary>
        /// Gets the path where procesos are stored in the disk.
        /// </summary>
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        /// <summary>
        /// Gets a list of a list of the basic information of PreParte without Parte.
        /// </summary>
        private List<List<string>> getPrePartesSinParte()
        {
            string query = @"SELECT 
                                    PreParte.numeroP, 
                                    PreParte.idEmpresa, 
                                    Empresas.nombreCortoE    
                            FROM 
                                    PreParte
                            LEFT JOIN 
                                    Partes ON Partes.numeroP = PreParte.numeroP
                            INNER JOIN 
                                    Empresas ON PreParte.idEmpresa = Empresas.idEmpresa
                            WHERE 
                                    Partes.numeroP IS NULL;";
            List<string> columna = new List<string> { "numeroP", "idEmpresa", "nombreCortoE" };
            return _context.readQueryList(query, columna);
        }
        /// <summary>
        /// With the list of basic information of the PrePartes it adds to it the files on the disk
        /// of the folder of the PreParte number.
        /// </summary>
        /// <param name="losPrePartes">A list of lists of the basic information of the PrePartes</param>
        private void getArchivosPrePartes(List<List<string>> losPrePartes)
        {
            for (int i = 0; i < losPrePartes.Count; i++)
            {
                string dirArchivos = System.IO.Path.Combine(pathProcesos, losPrePartes[i][0], "txt");
                if (!Directory.Exists(dirArchivos))
                {
                    losPrePartes[i].Add("No existe Carpeta Archivos");
                }
                else
                {
                    var archivosEnCarpeta = Directory.GetFiles(dirArchivos).ToList();
                    var carpetasEnDirectorio = Directory.GetDirectories(dirArchivos).ToList();
                    foreach (string unaCarpeta in carpetasEnDirectorio)
                    {
                        archivosEnCarpeta.AddRange(Directory.GetFiles(unaCarpeta).ToList());
                    }

                    losPrePartes[i].Add("");
                    foreach (string dirArchivo in archivosEnCarpeta)
                    {
                        string archivo = System.IO.Path.GetFileName(dirArchivo);
                        losPrePartes[i][3] += " | " + archivo;
                    }
                }
            }
        }
        /// <summary>
        /// With the raw list of basic information of PrePartes creates a PreParteSeleccion list and returns it
        /// </summary>
        /// <param name="theUpperList">A list of lists of the basic information of the PrePartes</param>
        private List<PreParteSeleccion> createListPPS(List<List<string>> theUpperList)
        {
            var thePPSList = new List<PreParteSeleccion>();
            foreach (List<string> oneList in theUpperList)
            {
                thePPSList.Add(crearPPS(oneList));
            }
            return thePPSList;
        }
        /// <summary>
        /// With the raw list of basic information of PrePartes creates a PreParteSeleccion list and returns it.
        /// </summary>
        /// <param name="theList">A list of the basic information of the PrePartes</param>
        private PreParteSeleccion crearPPS(List<string> theList)
        {
            int numeroP = stringToInt(theList[0]);
            int idEmpresa = stringToInt(theList[1]);
            string nombreE = theList[2];
            var listaArchivos = new List<string>();
            if (theList[3] != "No existe Carpeta Archivos")
            {
                listaArchivos = theList[3].Split('|').ToList().Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }
            else
            {
                listaArchivos.Add(theList[3]);
            }
            var PPS = new PreParteSeleccion(numeroP, idEmpresa, nombreE, listaArchivos);
            return PPS;
        }
        /// <summary>
        /// Transforms a string type variable to a int type. If it can't it returns 0.
        /// </summary>
        /// <param name="valor">A string of only numbers</param>
        private int stringToInt(string valor)
        {
            int elNumero = 0;
            bool esNumero = Int32.TryParse(valor, out elNumero);
            return elNumero;
        }
        /// <summary>
        /// With the selected value in the comboBox PrePartesToUse it pass it to the PreParteProcesos2 window.
        /// </summary>
        private void finalElection_Click(object sender, RoutedEventArgs e)
        {
            string messageToSend = PrePartesToUse.SelectedValue.ToString();
            bool isANumber = false;
            int theNumber = 0;
            if(messageToSend == null) 
            { 
                MessageBox.Show("Número PreParte invalido.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error); 
            }
            else
            {
                isANumber = Int32.TryParse(messageToSend, out theNumber);
            }

            if(theNumber > 0)
            {
                PreParteProcesos2 newWindow = new PreParteProcesos2(theNumber);
                newWindow.Show();

                // Close the current window
                this.Close();
            }
            else
            {
                MessageBox.Show("Error de número de PreParte.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
        }
    }
}
