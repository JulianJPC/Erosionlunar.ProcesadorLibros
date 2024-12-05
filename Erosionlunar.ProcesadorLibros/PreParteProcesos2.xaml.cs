using Erosionlunar.ProcesadorLibros.DB;
using Erosionlunar.ProcesadorLibros.Models;
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
    /// Interaction logic for PreParteProcesos2.xaml
    /// </summary>
    public partial class PreParteProcesos2 : Window
    {
        private readonly DBConector _context;
        private readonly string pathProcesos;
        private PreParteFormulario elFormulario;
        public PreParteProcesos2(int preParteNumero)
        {
            _context = new DBConector();
            pathProcesos = getPathProcesos();
            elFormulario = new PreParteFormulario();
            elFormulario.setIdPreParte(preParteNumero.ToString());
            elFormulario.setIdEmpresa(getIdEmpresaPreParte(elFormulario.numeroP));
            var theLibrosRaw = getLibrosEmpresa(elFormulario.idEmp);
            elFormulario.setLibros(turnIntoLibros(theLibrosRaw));
            elFormulario.setStartingFiles(getArchivosPreParte(elFormulario.numeroP));

            InitializeComponent();
        }

        private List<Libro> turnIntoLibros(List<List<string>> rawInfo)
        {
            var theList = new List<Libro>();
            foreach (var libro in rawInfo)
            {
                theList.Add(new Libro(libro));
            }
            return theList;
        }
        private void volverSeleccion_Click(object sender, RoutedEventArgs e)
        {
            PreParteProcesos newWindow = new PreParteProcesos();
            newWindow.Show();

            // Close the current window
            this.Close();
        }
        private List<string> getArchivosPreParte(string idPreParte)
        {
            string dirArchivos = System.IO.Path.Combine(pathProcesos, idPreParte, "txt");
            var archivosEnCarpeta = Directory.GetFiles(dirArchivos).ToList();
            var carpetasEnDirectorio = Directory.GetDirectories(dirArchivos).ToList();
            foreach (string unaCarpeta in carpetasEnDirectorio)
            {
                archivosEnCarpeta.AddRange(Directory.GetFiles(unaCarpeta).ToList());
            }

            return archivosEnCarpeta;
        }
        private string getPathProcesos()
        {
            string query = "SELECT valores from MainFrame WHERE idMainFrame = 1;";
            string columna = "valores";
            return _context.readQuerySimple(query, columna)[0];
        }
        private List<List<string>> getLibrosEmpresa(string IdEmpresa)
        {
            string query = "SELECT IdLibro, NombreL, NombreArchivoL, Codificacion  FROM PreParte WHERE IdEmpresa = @theID;";
            List<string> columns = new List<string> { "IdLibro", "NombreL", "NombreArchivoL", "Codificacion" };
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { IdEmpresa };
            return _context.readQueryList(query, parameters, values, columns);
        }
        private string getIdEmpresaPreParte(string numberPreParte)
        {
            string query = "SELECT idEmpresa FROM PreParte WHERE numeroP = @theID;";
            string column = "idEmpresa";
            List<string> parameters = new List<string> { "@theID" };
            List<string> values = new List<string> { numberPreParte };
            string response = "0";
            List<string> resultQuery = _context.readQuerySimple(query, parameters, values, column);
            if(resultQuery.Count == 1)
            {
                response = resultQuery[0];
            }
            return response;
        }
    }
}
