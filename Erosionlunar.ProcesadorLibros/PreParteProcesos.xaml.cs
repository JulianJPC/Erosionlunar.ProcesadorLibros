using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
using Mysqlx.Cursor;
using Mysqlx.Prepare;
using System;
using System.Collections.Generic;
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
            addToPreParteCombo(objectsList);
            DG1.ItemsSource = objectsList;
            
        }

        private void addToPreParteCombo(List<PreParteSeleccion> theList)
        {
            foreach(PreParteSeleccion onePP in theList)
            {
                PrePartesToUse.Items.Add(onePP.NumeroP);
            }
        }
        private List<PreParteSeleccion> createListPPS(List<List<string>> theUpperList)
        {
            var thePPSList = new List<PreParteSeleccion>();
            foreach(List<string> oneList in theUpperList)
            {
                thePPSList.Add(crearPPS(oneList));
            }
            return thePPSList;
        }
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
        private void getArchivosPrePartes(List<List<string>> losPrePartes)
        {
            for(int i = 0; i < losPrePartes.Count; i++)
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
                    foreach(string unaCarpeta in carpetasEnDirectorio)
                    {
                        archivosEnCarpeta.AddRange(Directory.GetFiles(unaCarpeta).ToList());
                    }
                    
                    losPrePartes[i].Add("");
                    foreach(string dirArchivo in archivosEnCarpeta)
                    {
                        string archivo = System.IO.Path.GetFileName(dirArchivo);
                        losPrePartes[i][3] += " | " + archivo;
                    }
                    
                    
                }
            }
        }
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
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        private int stringToInt(string valor)
        {
            int elNumero = 0;
            bool esNumero = Int32.TryParse(valor, out elNumero);
            return elNumero;
        }

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
